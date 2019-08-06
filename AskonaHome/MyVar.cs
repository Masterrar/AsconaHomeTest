using AskonaHome;
using System;

using System;
using System.Collections.Generic;
using System.IO;

namespace Askona.Home.Marketplace.Services.Implementation.MyBackground
{
    public class CompanyFeedAcceptorBackgroundService
    {
        private readonly IDBMarketplaceService _marketplace = new DBMarketplaceService();
        public CompanyFeedAcceptorBackgroundService()
        {

        }
        

        
        public async bool Process()
        {
            // Модель компании
            MarketplaceCompany company;
            // Отчет по валидации 
            MarketplaceValidationReport report = null;
            // Получить задачу для обработки фида.
            var updateTask = _marketplace.GetNextYmlUpdateTask();
            var feedAcceptor = new FabricFeedAcceptor().Create(updateTask);
            var validationWarning = false;
            var link = updateTask.YMLLink;
            byte[] bytes;
            try
            {
                // Получить ссылку на Yml - файл.
                
                bytes             = await feedAcceptor.Download(updateTask.YMLLink);
                validationWarning = feedAcceptor      .Valid(bytes);
                company           = feedAcceptor      .Parse(bytes);
                

                _marketplace.UpdateCompany(company);

                // Если все успешно, сохраним отчет по валидации
                _marketplace.MarkYMLUpdateTaskAsCompleted(updateTask.Id, report?.ExtractReports(), validationWarning);
            }
            catch (Exception ex)
            {
                // Если были ошибки, отметим задачу, как упавшую и сохраним отчет валидации.
                _marketplace.MarkYMLUpdateTaskAsFailed(updateTask.Id, ex.ToString(), report?.ExtractReports());
            }

            return true;
        }
    }

    public class MarketplaceValidationReport
    {
        private readonly List<string> _reports = new List<string>();

        public MarketplaceValidationReportJson ExtractReports()
        {
            if (_reports == null || _reports.Count == 0)
                return null;

            return _reports != null ? _reports.ToArray() : new string[0];
        }
    }

    public class YMLUpdateTask
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string YMLLink { get; set; }
        public DateTime CreateTime { get; set; }
        public int Priority { get; set; }
        public YMLUpdateTaskStatus Status { get; set; }
        public string FailMessage { get; set; }
        public UploadFiletype FileType { get; set; }
        public YMLDownloadSource DownloadSource { get; set; }
    }
}

