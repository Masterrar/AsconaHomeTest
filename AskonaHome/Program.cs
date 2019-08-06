using System;

using System;
using System.Collections.Generic;
using System.IO;

namespace Askona.Home.Marketplace.Services.Implementation.Background
{
    class CompanyFeedAcceptorBackgroundService
    {
        private readonly IDBMarketplaceService _marketplace = new DBMarketplaceService();

        public bool Process()
        {
            // Модель компании
            MarketplaceCompany company;
            // Отчет по валидации 
            MarketplaceValidationReport report = null;
            // Получить задачу для обработки фида.
            var updateTask = _marketplace.GetNextYmlUpdateTask();

            try
            {
                // Получить ссылку на Yml - файл.
                var link = updateTask.YMLLink;
                var validationWarning = false;

                byte[] bytes = null;

                // Загрузить файл.
                if (updateTask.DownloadSource == YMLDownloadSource.CDN)
                    bytes = new CDNService().DownloadBytes(link);
                else if (updateTask.DownloadSource == YMLDownloadSource.AutoFeed)
                    bytes = new FeedDownloadService().DownloadFromRemoteSource(updateTask.YMLLink);

                if (bytes == null || bytes.Length == 0)
                    throw new Exception(
                        $"yml download failed for companyId {updateTask.CompanyId}");
                // Валидация
                using (var validationStream = new MemoryStream(bytes))
                {
                    if (updateTask.FileType == UploadFiletype.YML)
                    {
                        var reports = new XMLServiceValidator().ValidateYmlFeed(validationStream);
                        // Превью модель для валидации
                        var preview = new FeedPreviewModel
                        {
                            // Валидация компании
                            CompanyValidationMessages = reports.shop.InvalidObjects
                                .Select(invalidObject => (CompanyReportModel)invalidObject.InvalidFields)
                               .ToList(),
                            // Валидация товаров
                            OffersReport =
                                reports.offers.InvalidObjects.Select(invalidOffer => (OfferReportModel)invalidOffer)
                                    .ToList(),
                            // Валидация категорий товаров
                            CategoriesReport =
                                reports.categories.InvalidObjects
                                    .Select(invalidCategory => (CategoryReportModel)invalidCategory)
                                    .ToList()
                        };
                        validationWarning = !preview.Valid;
                        // Сохранить отчет валидации.
                        new XMLServiceValidator().SaveReport(preview, updateTask.CompanyId);
                    }

                    // Распарсить фид компании.
                    using (var stream = new MemoryStream(bytes))
                        company = MarketplaceParser.From(stream, updateTask.FileType, out report);
                }

                if (company == null)
                    throw new Exception("unknown_feed_format");

                // Обновление данных
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
