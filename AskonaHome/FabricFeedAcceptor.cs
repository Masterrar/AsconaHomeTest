using System;
using System.Collections.Generic;
using System.Text;

namespace Askona.Home.Marketplace.Services.Implementation.Background
{
    public class FabricFeedAcceptor
    {
        public IYmlLoader Loader { get; }
        public IFeedValidator Validator { get; }

        public FabricFeedAcceptor()
        {

        }
        public FeedAccept Create(YMLUpdateTask taskInfo)
        {
            FeedAccept feedAcceptor = new FeedAccept()
            {
                Loader = PrepareLoader(taskInfo.DownloadSource),
                Validator = PrepareValidator(taskInfo.FileType)
            };
        }
    
        
            
        }

        private IYmlLoader PrepareLoader(YMLDownloadSource loaderSource)
        {
            switch (loaderType)
            {
                //...
            }
        }
        private IYmlLoader PrepareValidator(UploadFileType fileType)
        {
            switch (validatorType)
            {
                //...
            }
        }
        private IYmlParser PrepareParser(UploadFileType fileType)
        {
            switch (fileType)
            {
                //...
            }
        }   

}

