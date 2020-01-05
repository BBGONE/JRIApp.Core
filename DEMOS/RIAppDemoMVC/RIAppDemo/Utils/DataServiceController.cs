﻿using Microsoft.AspNetCore.Mvc;
using RIAPP.DataService.Core;
using RIAPP.DataService.Core.CodeGen;
using RIAPP.DataService.Core.Types;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace RIAppDemo.Utils
{
    [Route("[controller]/[action]")]
    [ApiController]
    public abstract class DataServiceController<TService> : ControllerBase
        where TService : BaseDomainService
    {
        private readonly TService _DomainService;

        public DataServiceController(TService domainService)
        {
            _DomainService = domainService;
        }

        protected TService DomainService
        {
            get { return _DomainService; }
        }

        #region CodeGen
        [ActionName("typescript")]
        [HttpGet]
        public ActionResult GetTypeScript()
        {
            string url = $"{ControllerContext.HttpContext.Request.Path}{ControllerContext.HttpContext.Request.QueryString}";
            DateTime now = DateTime.Now;
            var comment = $"\tGenerated from: {url} on {now:yyyy-MM-dd} at {now:HH:mm}\r\n\tDon't make manual changes here, they will be lost when this interface will be regenerated!";
            var content = DomainService.ServiceCodeGen(new CodeGenArgs("ts") { comment = comment });
            var res = new ContentResult();
            res.ContentType = MediaTypeNames.Text.Plain;
            res.Content = content;
            return res;
        }

        [ActionName("xaml")]
        [HttpGet]
        public ActionResult GetXAML(bool isDraft = true)
        {
            var content = DomainService.ServiceCodeGen(new CodeGenArgs("xaml") { isDraft = isDraft });
            var res = new ContentResult();
            res.ContentType = MediaTypeNames.Text.Plain;
            res.Content = content;
            return res;
        }

        [ActionName("csharp")]
        [HttpGet]
        public ActionResult GetCSharp()
        {
            var content = DomainService.ServiceCodeGen(new CodeGenArgs("csharp"));
            var res = new ContentResult();
            res.ContentType = MediaTypeNames.Text.Plain;
            res.Content = content;
            return res;
        }

        [ActionName("code")]
        [HttpGet("{lang?}")]
        public ActionResult GetCode(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                lang = this.Request.Query["lang"];
            }

            switch (lang?.ToLowerInvariant())
            {
                case "ts":
                case "typescript":
                    return GetTypeScript();
                case "xml":
                case "xaml":
                    return GetXAML();
                case "cs":
                case "csharp":
                    return GetCSharp();
                default:
                    throw new Exception(string.Format("Unknown lang argument: {0}", lang));
            }
        }
        #endregion

        [ActionName("permissions")]
        [HttpGet]
        public async Task<ActionResult> GetPermissions()
        {
            Permissions res = await DomainService.ServiceGetPermissions();
            return new ChunkedResult<Permissions>(res, _DomainService.Serializer);
        }

        [ActionName("query")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PerformQuery([FromBody] QueryRequest request)
        {
            var res = await DomainService.ServiceGetData(request);
            // return new JsonResult(res);
            return new ChunkedResult<QueryResponse>(res, DomainService.Serializer);
        }

        [ActionName("save")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Save([FromBody] ChangeSetRequest changeSet)
        {
            var res = await DomainService.ServiceApplyChangeSet(changeSet);
            return new ChunkedResult<ChangeSetResponse>(res, DomainService.Serializer);
        }

        [ActionName("refresh")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Refresh([FromBody] RefreshInfoRequest refreshInfo)
        {
            var res = await DomainService.ServiceRefreshRow(refreshInfo);
            return new ChunkedResult<RefreshInfoResponse>(res, DomainService.Serializer);
        }

        [ActionName("invoke")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Invoke([FromBody] InvokeRequest invokeInfo)
        {
            var res = await DomainService.ServiceInvokeMethod(invokeInfo);
            return new ChunkedResult<InvokeResponse>(res, DomainService.Serializer);
        }
    }
}
