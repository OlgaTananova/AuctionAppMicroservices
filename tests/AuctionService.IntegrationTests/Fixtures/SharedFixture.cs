using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuctionService.IntegrationTests.Fixtures
{

    // CollectionDefinition attribute is used to group tests that share the CustomWebAppFactory fixture,
    // allowing the fixture to be initialized once and reused across multiple test classes.

    [CollectionDefinition("SharedCollection")]
    public class SharedFixture: ICollectionFixture<CustomWebAppFactory>
    {
    }
}
