using System;
using System.Web.Http;
using System.Web.Mvc;
using AudioToolNew.Areas.HelpPage.ModelDescriptions;
using AudioToolNew.Areas.HelpPage.Models;
using System.Web.Http.Dispatcher;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http.Description;
using System.Linq;

namespace AudioToolNew.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        private const string ErrorViewName = "Error";

        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        public HttpConfiguration Configuration { get; private set; }

        public ActionResult Index()
        {

            IAssembliesResolver assembliesResolver = Configuration.Services.GetAssembliesResolver();
            var controllerlist =
                Configuration.Services.GetHttpControllerTypeResolver().GetControllerTypes(assembliesResolver);
            List<string> dirList = GetDirList(controllerlist);
            return View(dirList);
        }

        public ActionResult ApiList(string dirName)
        {
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            var list = Configuration.Services.GetApiExplorer().ApiDescriptions;
            Collection<ApiDescription> duplicatelist = new Collection<ApiDescription>();
            foreach (var item in list)
            {
                if (item.GetAreaName() == dirName)
                {
                    item.GetFriendlyId();
                    if (!ContainsSameApi(item, duplicatelist))
                    {
                        duplicatelist.Add(item);
                    }
                }

            }
            return View(duplicatelist);
        }

        private List<string> GetDirList(ICollection<Type> controllerlist)
        {
            List<string> areaList = new List<string>();
            foreach (var item in controllerlist)
            {
                //获取controller的fullname
                string controllerFullName = item.FullName;
                string[] dirName = controllerFullName.Split('.');
                if (dirName.Length > 3)
                {
                    areaList.Add(dirName[2]);
                }
            }
            return areaList.Distinct().ToList();
        }

        private bool ContainsSameApi(ApiDescription item, Collection<ApiDescription> duplicatelist)
        {
            return duplicatelist.Any(api => item.RelativePath == api.RelativePath && item.HttpMethod.Method == api.HttpMethod.Method);
        }

        public ActionResult Api(string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    //防止生成帮助文档时将area、category作为了Uri参数
                    var lists = apiModel.UriParameters.Where(t => t.Name.Equals("area") || t.Name.Equals("category")).ToList();

                    for (int i = 0; i < lists.Count(); i++)
                    {
                        apiModel.UriParameters.Remove(lists[i]);
                    }
                    return View(apiModel);
                }
            }

            return View(ErrorViewName);
        }

        public ActionResult ResourceModel(string modelName)
        {
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = Configuration.GetModelDescriptionGenerator();
                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    return View(modelDescription);
                }
            }

            return View(ErrorViewName);
        }
    }
}