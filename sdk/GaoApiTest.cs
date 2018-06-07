namespace Cloud.Governance.Samples.Sdk
{
    #region using directives

    using System;
    using AvePoint.GA.WebAPI;
    using Xunit;
    using Xunit.Abstractions;

    #endregion using directives

    /// <summary>
    /// GaoApiTest class shows the initial process of the GaoApi sdk, please refer to the
    /// CombineGaoApi test case for detail process. You should replace the datacenter, username and
    /// password to your own to use the API
    /// </summary>
    public class GaoApiTest : IClassFixture<GaoApiClassFixtrue>
    {
        public GaoApiTest(ITestOutputHelper testOutput, GaoApiClassFixtrue fixtrue)
        {
            this.fixtrue = fixtrue;
            this.output = testOutput;
        }

        private readonly GaoApiClassFixtrue fixtrue;
        private readonly ITestOutputHelper output;

        [Trait("Owner", "Baron")]
        [Theory(DisplayName = "GaoApi.Init 001")]
        [InlineData(Region.SoutheastAsia, "baron@baron.sapce", "baron@baron.space")]
        public void Init001(Region region, String username, String password)
        {
            Assert.Equal(1, this.fixtrue.ExecutionCount);
            Assert.True(this.fixtrue.InitStatus);
            this.output.WriteLine("GaoApi class should be initialized once");
            GaoApi.Init(region, username, password);
        }

        [Trait("Owner", "Baron")]
        [Theory(DisplayName = "GaoApi.Init 001")]
        [InlineData(Region.USGov)]
        public void GetApiUrlByRegion(Region region)
        {
            this.output.WriteLine("GetApiUrlByRegion testing");
            var usGovDatacentre = GaoApi.GetApiUrlByRegion(region);
            Assert.Equal("https://gausgovapi.avepointonlineservices.com", usGovDatacentre);
        }

        [Trait("Owner", "Baron")]
        [Theory(DisplayName = "GaoApi")]
        [InlineData(Region.SoutheastAsia, "baron@baron.sapce", "baron@baron.space")]
        public void CombineGaoApi(Region region, String username, String password)
        {
            GaoApi.Init(region, username, password);
            var service = GaoApi.Create<ICommonService>();
            Assert.NotNull(service);
            this.output.WriteLine("using GaoApi.Create to get a instance of client proxy");
        }

        [Trait("Owner", "Baron")]
        [Fact(DisplayName = "GaoApi.Create")]
        public void Create()
        {
            var service = GaoApi.Create<ICommonService>();
            Assert.NotNull(service);
            this.output.WriteLine("using GaoApi.Create to get a instance of client proxy");
        }

        [Trait("Owner", "Baron")]
        [Fact(DisplayName = "GaoApi.Init")]
        public void Init()
        {
            //The code style here is leverage the xUnit test style, you should gurantee that
            //your GaoApi.Init method run once before you can invoke the other GaoApi interface.
            //you can find the Init sample in the GaoApiClassFixture.cs
            Assert.Equal(1, this.fixtrue.ExecutionCount);
            Assert.True(this.fixtrue.InitStatus);
            this.output.WriteLine("GaoApi class should be initialized once");
        }
    }
}