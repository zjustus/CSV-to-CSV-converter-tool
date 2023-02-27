using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace SNF_Import_Creator
{
    internal class JsonProcessor
    {
        // Convertsd a JSON File into a FundDict Object
        public static FundDict? ProcessJSON(string jsonFile)
        {
            //CsvDef defObject;
            //List<ColumnDef> columnObjects = new();
            
            // Parse the JSON input file
            var jsonText = File.ReadAllText(jsonFile);
            JsonElement jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonText);


            // Input Accounts
            List<Account> inputFunds = new();
            try
            {
                JsonElement inputJson = jsonObject.GetProperty("inputFunds");
                inputFunds = AccountList(inputJson);
            } 
            catch
            {
                throw new Exception("Input Fund Missing Data");
            }

            // Elvanto Account
            List<TitheAccount> elvantoFunds = new();
            try
            {
                bool hasElvanto = jsonObject.TryGetProperty("elvanto", out JsonElement elvantoJson);
                if (hasElvanto) { elvantoFunds = TitheAccountList(elvantoJson); }
            }
            catch
            {
                throw new Exception("An Elvanto Fund is Missing Data");
            }

            // Tithly Account
            List<TitheAccount> tithlyFunds = new();
            try
            {
                bool hasTithly = jsonObject.TryGetProperty("tithly", out JsonElement tithlyJson);
                if (hasTithly) { tithlyFunds = TitheAccountList(tithlyJson); }
            }
            catch {
                throw new Exception("A Tithly Account is Missing Data");
            }

            // PushPay Account
            List<TitheAccount> pushpayFunds = new();
            try
            {
                bool hasPushpay = jsonObject.TryGetProperty("pushpay", out JsonElement pushpayJson);
                if (hasPushpay) { pushpayFunds = TitheAccountList(pushpayJson); }
            }
            catch
            {
                throw new Exception("A Pushpay Account is Missing Data");
            }

            return new( elvantoFunds, tithlyFunds, pushpayFunds, inputFunds);
        }

        // Converts a JsonElement into a TitheAccount Object
        static List<TitheAccount> TitheAccountList(JsonElement input)
        {
            List<TitheAccount> someAccount = new();
            foreach(JsonElement i in input.EnumerateArray()) {
                if(
                    i.TryGetProperty("name", out JsonElement theName) &&
                    i.TryGetProperty("co", out JsonElement theCo) &&
                    i.TryGetProperty("fund", out JsonElement theFund) &&
                    i.TryGetProperty("department", out JsonElement theDepartment) &&
                    i.TryGetProperty("account", out JsonElement theAccount)
                )
                {
                    someAccount.Add(new(
                        theName.GetRawText(),
                        CoNumber: theCo.GetRawText(),
                        FundNumber: theFund.GetRawText(),
                        DepartmentNumber: theDepartment.GetRawText(),
                        AccountNumber: theAccount.GetRawText()
                    ));
                }
                else throw new Exception("Tithe account missing data");
            }

            return someAccount;
        }

        // Converts a Json Element into an Account Object
        static List<Account> AccountList(JsonElement input)
        {
            List<Account> someAccount = new();
            foreach(JsonElement i in input.EnumerateArray())
            {
                if (
                    i.TryGetProperty("co", out JsonElement theCo) &&
                    i.TryGetProperty("fund", out JsonElement theFund) &&
                    i.TryGetProperty("department", out JsonElement theDepartment) &&
                    i.TryGetProperty("account", out JsonElement theAccount)
                )
                {
                    someAccount.Add(new(
                        theCo.GetRawText(),
                        theFund.GetRawText(),
                        theDepartment.GetRawText(),
                        theAccount.GetRawText()
                    ));
                }
                else throw new Exception("Input Account Missing Data");
            }
            return someAccount;
        }   
    }
}
