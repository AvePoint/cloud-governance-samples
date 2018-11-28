namespace Cloud.Governance.Samples.Sdk
{
    #region using directives

    using System;
    using AvePoint.GA.WebAPI;

    #endregion using directives

    /// <summary>
    /// GaoApiClassFixtrue is a global initial class for GaoApi client context initializaion.
    /// </summary>
    public class GaoApiClassFixtrue
    {
        public GaoApiClassFixtrue()
        {
            this.InitStatus = true;
            GaoApi.Init(Region.SoutheastAsia, "baron@baron.space", "baron@baron.space");
            this.ExecutionCount++;
        }

        public Boolean InitStatus { get; }
        public Int32 ExecutionCount { get; }
    }
}