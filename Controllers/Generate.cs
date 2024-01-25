using IndexInfo.Contexts;
using IndexLicenseKeyGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.ComponentModel;
using System.Text;
using System.IO;
using IndexInfo.Models;
using System.Reflection;
using License = IndexInfo.Models.License;
using IndexInfo.LicenseHandler;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace IndexInfo.Controllers
{
    public class Generate : Controller
    {
        private readonly MainEntity mainEntitys;
        private readonly Random _random = new Random();
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment environment;

        public Generate(MainEntity mainEntity, IToastNotification toastNotification, IWebHostEnvironment environment)
        {
            mainEntitys = mainEntity;
            _toastNotification = toastNotification;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult generate()
        {
            for (; ; )
            {
                var GetProductID = RandomString(18, false);
                var IfexistsTenantId = mainEntitys.licenses.Where(x=>x.ProductID== GetProductID).FirstOrDefault();
                 ViewBag.ProductID = GetProductID;
                 GetListOfModule();
                 GetListCompanyName();
                if (IfexistsTenantId != null) { continue; } else { break; }
            }
            return View();
        }
        [HttpPost]
        public IActionResult generate(string txtProductID, string txtDays, string txtUtilization, string Tenant,string ListOfModuleValue,string Domain,string LicenseType,string TenantID)
        {
            string strErrorMsg = ""; string strCurrentMachine = "";
            string getNewLicense = "";
            string strPublicKey = "", strPrivateKey = "";

            if (string.IsNullOrEmpty(Tenant))
            {
                strErrorMsg += "Please select the Tenant Name";
            }
            if (string.IsNullOrEmpty(txtProductID))
            {
                strErrorMsg += "Please enter valid product id";
            }
            if (Convert.ToInt16(txtDays) <= 0)
            {
                strErrorMsg += "Please enter valid expiry days";
            }

            if (!string.IsNullOrEmpty(strErrorMsg))
            {
                _toastNotification.AddErrorToastMessage(strErrorMsg);
            }
            else
            {
                Dictionary<string, string> dctKeyValuePair = new Dictionary<string, string>();
                dctKeyValuePair = IndexLicenseKeyGenerator.LicenseKey.GenerateKeyPair();

                foreach (var item in dctKeyValuePair)
                {
                    strPrivateKey = item.Key.ToString();
                    strPublicKey = item.Value.ToString();
                }
                getNewLicense = IndexLicenseKeyGenerator.LicenseKey.GenerateLicense(strPrivateKey, Tenant.Trim(), strCurrentMachine, Convert.ToInt32(txtDays.Trim()), LicenseType.Trim(), Convert.ToInt32(txtUtilization.Trim()), txtProductID, Domain, ListOfModuleValue, TenantID);
				ViewBag.ClientName = Tenant;
                GetListOfModule();
                GetListCompanyName();
            }
            var SetTheKeyValues = new Key
            {
                PublicKey = strPublicKey,
                PrivateKey = strPrivateKey,
                LicenseDetails = getNewLicense.ToString()

            };
            return Json(SetTheKeyValues);
        }
        public void GetListOfModule()
        {
            var GetListOfModule = mainEntitys.moduleMasters.ToList();
            ViewBag.GetListOfModule = GetListOfModule;
        }
        public void GetListCompanyName()
        {
            var GetListOfCompanyName = mainEntitys.tenantMasters.ToList();
            ViewBag.GetListOfCompanyName = GetListOfCompanyName;
        }
        [HttpPost]
        public IActionResult GenerateLicense(string LicenseDetails, string Tenant, string txtProductID, string txtDays,
            string txtUtilization, string ListOfModuleValue, string LicenseType, string txtDomain,string txtSerialKey,string TenantID)
        {
            string strFileName = "License.xml";
            string strLicfolderPath = GetLicenseFolderLocation(txtProductID);
            string ddlLicenseType = Request.Form["LicenseType"].ToString();
            string strErrorMsg = "";

            //if (!string.IsNullOrEmpty(strLicfolderPath))
            //{
            //    if (string.IsNullOrEmpty(Tenant))
            //    {
            //        strErrorMsg += "Please enter valid customer name or email id";
            //    }
            //    if (string.IsNullOrEmpty(txtProductID))
            //    {
            //        strErrorMsg += "Please enter valid product id";
            //    }
            //    if (Convert.ToInt16(txtDays) <= 0)
            //    {
            //        strErrorMsg += "Please enter valid expiry days";
            //    }
            //    if (string.IsNullOrEmpty(LicenseDetails))
            //    {
            //        strErrorMsg += "Please Generate the LicenseDetails";
            //    }
            //    if (string.IsNullOrEmpty(txtUtilization))
            //    {
            //        strErrorMsg += "Please Enter the Utilization";
            //    }
            //    if (string.IsNullOrEmpty(ListOfModuleValue))
            //    {
            //        strErrorMsg += "Please Select the Module";
            //    }
            //    if (!string.IsNullOrEmpty(strErrorMsg))
            //    {
            //        _toastNotification.AddErrorToastMessage(strErrorMsg);
            //    }
            //    else
            //    {
            string path = Path.Combine(this.environment.WebRootPath, "Downloads");

            LicenseKey.GenerateXMLLicenseFile(LicenseDetails, path, strFileName);
                    string[] ListOfModule = ListOfModuleValue.Split(',');
                    DateTime date = DateTime.Now;
                    date = date.AddDays(Convert.ToInt32(txtDays));
                    foreach (var GetItem in ListOfModule)
                    {
                        var AddListOfData = new License()
                        {
                            ProductID = txtProductID,
                            TenantId = TenantID,
                            ValidDays = Convert.ToInt32(txtDays),
                            Utilization = Convert.ToInt32(txtUtilization),
                            LicenseType = ddlLicenseType,
                            Module = GetItem,
                            GenarationOn = DateTime.Now,
                            ExpiredOn = date,
                            Domain = txtDomain,
                        };
                        mainEntitys.licenses.Add(AddListOfData);
                        mainEntitys.SaveChanges();
                    }
                    _toastNotification.AddSuccessToastMessage("License generated successfully");
            //var bytes = stream.ToArray();
            //Response.Clear();
            //Response.ContentType = "text/xml";
            //Response.Headers.Add("Content-Disposition", "attachment;filename=example.xml");
            //Response.WriteAsync(System.Text.Encoding.UTF8.GetString(bytes));
            //var downloadPath = Path.Combine(path, "License.xml");
            return Json(new {path =  "/Downloads/License.xml", fileName = txtProductID+".xml"});
            ;
            //return PhysicalFile(downloadPath, "application/xml", txtProductID+".xml");            //}
            //}
            //else
            //{
            //    _toastNotification.AddErrorToastMessage("Please select a valid file location");
            //}
            //return RedirectToAction("generate", "Generate");
        }
        string GetLicenseFolderLocation(string txtProductID)
        {
            string folderPath = "";
            int Count = 1;
            folderPath = @"c:\Downloads\License\" + txtProductID;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }
        [HttpPost]
        public IActionResult ValidateKey(string LicenseDetails, string txtSerialKey, string txtClientName)
        {
            string strFileName = "IdxLicenseKey.xml";
            ViewBag.ClientName = txtClientName;
            if (LicenseDetails != null)
            {
                string strResult = LicenseKey.ValidateLicense(txtSerialKey, strFileName);
            }
            else
            {
                if (string.IsNullOrEmpty(txtSerialKey))
                {
                    _toastNotification.AddErrorToastMessage("Key cannot be empty or null");
                }
                else
                {
                    string strResult = LicenseKey.ValidateLicense(txtSerialKey, null);
                }
            }
            return RedirectToAction("generate", "Generate");
        }

        public string RandomString(int size, bool lowerCase = false)
        {

            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

    }
}
