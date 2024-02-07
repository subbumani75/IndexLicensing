using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Standard.Licensing.Security.Cryptography;
using Standard.Licensing;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using Standard.Licensing.Validation;
//using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using NToastNotify;
//using System.Windows.Forms;
//using System.ComponentModel;

namespace IndexLicenseKeyGenerator
{
    internal class LicenseKey
    {
        private  static   IToastNotification _toastNotification;
        static License newLicense;
        
        public LicenseKey() { }
        public LicenseKey(string key) {  }
        public  LicenseKey(IToastNotification toastNotification)
        {
            _toastNotification = toastNotification;
        }

        static string passPhrase = "6mML7vh~ZGiOv2$cGri^YBwEWG6aOzy9ko41Zet*GVqL%zqN";

        public static Dictionary<string,string> GenerateKeyPair()
        {
            Dictionary<string, string> strRetValue = new Dictionary<string, string>();

            KeyGenerator keyGenerator = KeyGenerator.Create();
            KeyPair keyPair = keyGenerator.GenerateKeyPair();
            
            string privateKey = keyPair.ToEncryptedPrivateKeyString(passPhrase);
            string publicKey = keyPair.ToPublicKeyString();

            Console.WriteLine("Private key: {0}", privateKey);
            Console.WriteLine("Public key : {0}", publicKey);
            strRetValue.Add(privateKey,publicKey);
            

            return strRetValue;
        }

        public static string GenerateLicense(string privateKey, string customerName, string deviceIdentifier, int days, string licenseType, int utilization,string ProductID,string Domain,string ListOfModuleValue,string TenantID, DateTime ExpioredOn)
        {
            //string strRetValue = "";

            DateTime expiryDate = DateTime.UtcNow.Date.AddDays(days);
            //string deviceIdentifier = Environment.MachineName;
            //string customerName = "Jonathan Crozier";
            ///licenseType
            
            newLicense = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .ExpiresAt(expiryDate)
                .As(licenseType == "Trial" ? LicenseType.Trial :LicenseType.Standard )
                
                .WithAdditionalAttributes(new Dictionary<string, string>
				{
						{ "ProductID", ProductID }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "Domain", Domain }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "TenantName", customerName }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "ValidDays", Convert.ToString(days) }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "Utilization", Convert.ToString(utilization) }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "LicenseType", licenseType }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "Module", ListOfModuleValue }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "TenantID", TenantID }
                })
                .WithAdditionalAttributes(new Dictionary<string, string>
                {
                        { "ExpioredOn",  ExpioredOn.ToString("yyyy-MM-dd HH:mm:ss")}
                })
                .LicensedTo((c) => c.Name = customerName)
                .WithMaximumUtilization(utilization)
                .CreateAndSignWithPrivateKey(privateKey, passPhrase);
                

            return newLicense.ToString();

        }

        public static void EncodeLicense(string license) 
        {
            string licenseKey = Base64UrlEncoder.Encode(license);

            Console.WriteLine(licenseKey);
        }

        public static void LoadLicense(string path)
        {
            using (var xmlReader = new XmlTextReader(path))
            {
                newLicense = License.Load(xmlReader);
            }
        }


        public static void GenerateXMLLicenseFile(string licenseDetails,string folderPath,string fileName)
        {
            //File.WriteAllText("License.lic", license.ToString(), Encoding.UTF8);  //"Indexlicense.xml"
            using (var xmlWriter = new XmlTextWriter(System.IO.Path.Combine(folderPath, fileName), Encoding.UTF8))
            {
                newLicense.Save(xmlWriter);
            }
        }

        public static void GenerateTextLicenseFile(string licenseDetails, string folderPath, string fileName)
        {
            //File.WriteAllText("License.lic", license.ToString(), Encoding.UTF8);  //"License.lic"
            System.IO.File.WriteAllText(System.IO.Path.Combine(folderPath, fileName), licenseDetails, Encoding.UTF8);            
        }

        public static string ValidateLicense(string? publicKey=null, string? filePath=null)
        {
            string strValid = "Valid"; string strErrorMessage = ""; string strExceptionMessage = "";
            string currentDeviceIdentifier = ""; // Environment.MachineName;
            string strPath = filePath;
            if (string.IsNullOrEmpty(publicKey))
            {
                 //_toastNotification.AddErrorToastMessage("Key cannot be empty or null");
            }
            else
            {
                try
                {
                    License license;

                    using (var xmlReader = new XmlTextReader(strPath))
                    {
                        license = License.Load(xmlReader);
                    }

                    var validationFailures =
                    license.Validate()
                           .ExpirationDate()
                           .And()
                           .Signature(publicKey)
                           .And()
                           .AssertThat(lic => // Check Device Identifier matches.
                               lic.AdditionalAttributes.Get("DeviceIdentifier") == currentDeviceIdentifier,
                               new GeneralValidationFailure()
                               {
                                   Message = "Invalid Device.",
                                   HowToResolve = "Contact the supplier to obtain a new license key."
                               })
                           .AssertValidLicense();

                    System.Threading.Thread.Sleep(2000);
                    try
                    {
                        bool bln = validationFailures.Any();
                    }
                    catch
                    { }
                    foreach (var failure in validationFailures)
                    {
                        strErrorMessage += failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve;
                       //strErrorMessage = validationFailures.First().Message;
                    }

                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(strErrorMessage))
                    { strExceptionMessage += ex.ToString(); }
                }
                if (!string.IsNullOrEmpty(strExceptionMessage))
                {  //strErrorMessage
                    strValid = strExceptionMessage;
                    //MessageBox.Show("Invalid key passed" + Environment.NewLine + Environment.NewLine + strExceptionMessage + Environment.NewLine + Environment.NewLine + "Please contact vendor for valid key", "License Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (!string.IsNullOrEmpty(strErrorMessage))
                {  //strErrorMessage
                    strValid = strErrorMessage;
                    //MessageBox.Show(strErrorMessage + Environment.NewLine + Environment.NewLine + "", "License Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            return strValid;
        }

       

    }

}
