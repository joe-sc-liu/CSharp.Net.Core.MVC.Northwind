using System;


namespace CSharp.Net.Core.MVC.Northwind.Models
{
    //https://ithelp.ithome.com.tw/articles/10194989
    public class ResultModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }


}
