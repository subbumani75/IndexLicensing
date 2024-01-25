using System.Windows.Forms;
using Standard.Licensing;
using Standard.Licensing.Validation;
using System.Xml;
using System.Data;
namespace IndexInfo.LicenseHandler
{
    public class RegisterLicense
    {
        public static string SerialKey = "";
        public static System.Data.DataTable dtLicenseFile = new DataTable();
        public static string UserORMachineID = Environment.MachineName;
        public static bool IsUserSessionActive = false;
        public static bool IsProductRegistered = false;
        public static bool IsLicenseValid = false;
        public static string RegisterUserFilePath = Path.Combine("C:\\Key\\License32", "IdxLicenseKey.xml");

        public static string ValidateLicense()
        {
            string strErrorMessage = ""; string strValue = "";
            if (!IsUserSessionActive) { IsUserSessionActive = HasSessionActive(RegisterUserFilePath, UserORMachineID); }
            if (!IsUserSessionActive) { strErrorMessage = "User ID " + UserORMachineID + " not registered in the system. Please register using IRegisterMyUser option"; }
            return strErrorMessage;
        }

        public static string IsValidLicense(string publicKey, string filePath)
        {

            string strValid = "Valid"; string strErrorMessage = ""; string strExceptionMessage = "";
            string currentDeviceIdentifier = ""; // Environment.MachineName;
            string strPath = filePath;
            if (string.IsNullOrEmpty(publicKey))
            {
                MessageBox.Show("Key cannot be empty or null", "Key Generation", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        private static DataTable GetRegistrationTable()
        {
            // Create sample Customers table, in order
            // to demonstrate the behavior of the DataTableReader.
            DataTable table = new DataTable();
            table.TableName = "Registration";
            // Create three columns; OrderID, CustomerID, and OrderDate.
            table.Columns.Add(new DataColumn("MachineID", typeof(System.String)));
            table.Columns.Add(new DataColumn("RegisteredBy", typeof(System.String)));
            table.Columns.Add(new DataColumn("RegisteredOn", typeof(System.DateTime)));
            table.Columns.Add(new DataColumn("Active", typeof(System.Boolean)));
            table.Columns.Add(new DataColumn("IsSesstionActive", typeof(System.Boolean)));
            table.Columns.Add(new DataColumn("SessionActiveFrom", typeof(System.DateTime)));

            // Set the OrderID column as the primary key column.
            table.PrimaryKey = new DataColumn[] { table.Columns[0] };
            table.AcceptChanges();
            return table;
        }

        public static string RegisterUser(string filePath, string machineID, string registeredBy, bool active, bool isSesstionActive)
        {

            DataTable licenseRegTable = new DataTable();
            string strReturn = "Valid"; string strFilter = String.Format("MachineID = '{0}'", machineID);
            //string strFilePath = Path.Combine(EpicorConstants.IdxLicenseFolderPath, EpicorConstants.IdxUserRegistrationFileName);
            bool bAddNewRow = true;
            licenseRegTable = LoadRegisterUser(filePath);

            if (licenseRegTable != null)
            {
                if (licenseRegTable.Rows.Count > 0)
                {
                    DataRow[] rows = licenseRegTable.Select(strFilter);
                    if (rows != null && rows.Length > 0)
                    {
                        rows[0]["IsSesstionActive"] = isSesstionActive;
                        rows[0]["SessionActiveFrom"] = (isSesstionActive) ? DateTime.Now : new DateTime();
                        licenseRegTable.AcceptChanges();
                        bAddNewRow = false;
                    }
                }
                if (bAddNewRow)
                {
                    DataRow workRow = licenseRegTable.NewRow();

                    workRow["MachineID"] = Convert.ToString(machineID);
                    workRow["RegisteredBy"] = Convert.ToString(registeredBy);
                    workRow["RegisteredOn"] = DateTime.Now;
                    workRow["Active"] = active;
                    workRow["IsSesstionActive"] = isSesstionActive;
                    workRow["SessionActiveFrom"] = (isSesstionActive) ? DateTime.Now : new DateTime();

                    licenseRegTable.Rows.Add(workRow);
                }


                using (StreamWriter fs = new StreamWriter(filePath)) // XML File Path
                {
                    licenseRegTable.WriteXml(fs);
                }

            }

            return strReturn;

        }



        public static DataTable LoadRegisterUser(string filePath)
        {
            //create the DataTable that will hold the data
            DataTable licenseRegTable = GetRegistrationTable();
            try
            {
                //open the file using a Stream
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    //use ReadXml to read the XML stream
                    licenseRegTable.ReadXml(stream);

                    //return the results
                    return licenseRegTable;
                }
            }
            catch (Exception ex)
            {
                return licenseRegTable;
            }
        }


        public static bool HasSessionActive(string filePath, string machineID)
        {
            bool bReturn = true;
            if (!File.Exists(filePath))
            { bReturn = false; }
            else
            {
                RegisterUser(filePath, machineID, "", true, true);
                IsUserSessionActive = true;
                if (!IsUserSessionActive) { bReturn = false; }
            }
            return bReturn;
        }

    } 

}
