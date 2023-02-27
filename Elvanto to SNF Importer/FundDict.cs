using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNF_Import_Creator
{
    internal class FundDict
    {
        public List<TitheAccount> ElvantoFunds { get; }
        public List<TitheAccount> TithlyFunds {get;}
        public List<TitheAccount> PushpayFunds {get;}
        public List<Account> InputFunds {get;}

        public FundDict(
            List<TitheAccount> ElvantoFunds,
            List<TitheAccount> TithlyFunds,
            List<TitheAccount> PushpayFunds,
            List<Account> InputFunds
        )
        {
            this.ElvantoFunds = ElvantoFunds;
            this.TithlyFunds = TithlyFunds;
            this.PushpayFunds = PushpayFunds;
            this.InputFunds = InputFunds;
        }

        public TitheAccount GetFundDetails(string titheAccount, string fundName)
        {
            List<TitheAccount> x;
            if (titheAccount == "elvanto") { x = ElvantoFunds; }
            else if (titheAccount == "tithly") { x = TithlyFunds; }
            else if (titheAccount == "pushpay") { x = PushpayFunds; }
            else throw new Exception("Invalid Input");

            TitheAccount? output = null;
            foreach(TitheAccount i in x)
            {
                if(i.FundName == fundName)
                {
                    output = i;
                    break;
                }
            }
            if(output is null) throw new Exception("Fund is not found in Dictionary");
            return output;
            
        }
    }

    internal class Account
    {
        public string CoNumber { get;}
        public string FundNumber { get;}
        public string DepartmentNumber { get;}
        public string AccountNumber { get; }


        public Account(
            string CoNumber,
            string FundNumber,
            string DepartmentNumber,
            string AccountNumber
        )
        {
            this.CoNumber = CoNumber;
            this.FundNumber = FundNumber;
            this.DepartmentNumber = DepartmentNumber;
            this.AccountNumber = AccountNumber;
        }
    }

    internal class TitheAccount : Account
    {
        public string FundName { get; }

        public TitheAccount(
            string FundName,
            string CoNumber,
            string FundNumber,
            string DepartmentNumber,
            string AccountNumber
        ) : base(
            CoNumber,
            FundNumber,
            DepartmentNumber,
            AccountNumber
        )
        {
            this.FundName = FundName;
        }
    }
}
