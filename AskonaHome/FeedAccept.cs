using System;
using System.Collections.Generic;
using System.Text;

namespace Askona.Home.Marketplace.Services.Implementation.Background
{
    public class FeedAccept
    {
        public IYmlLoader Loader { get; }
        public IFeedValidator Validator { get; }

        public IFeedParser Parser { get; }
        public FeedAcceptProcess()
        {

        }
        public void Start()
        {
            Prepare();
        }

        public async byte[] Download()
        {
            await Loader.Download();
        }
        public bool Valid(byte[] bytes)
        {
            return Validator.Valid(bytes);
        }
        public MarketplaceCompany Parse(byte[] bytes)
        {
            return Parser.Parse(bytes);
        }
        

    }
}
