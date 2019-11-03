﻿using System;
using System.Globalization;
using CSharp.Net.Core.MVC.Northwind.Middlewares;
using CSharp.Net.Core.MVC.Northwind.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Northwind.Service.Interfaces;
using System.Reflection;

namespace CSharp.Net.Core.MVC.Northwind.Controllers.Api
{
    //參考https://ithelp.ithome.com.tw/articles/10194989


    [Route("{culture=en-US}/api/[controller]")]
    [ApiController]
    [MiddlewareFilter(typeof(CultureMiddleware))]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _IProductsService;
        private readonly Serilog.ILogger _logger;
        private readonly IStringLocalizer _sharedLocalizer;
        private readonly IStringLocalizer _localizer;
        private readonly IStringLocalizer _stringLocalizerFactory;
        public ProductsController(IProductsService iProductsService
            , Serilog.ILogger logger
            , IStringLocalizer<SharedResource> sharedLocalizer
            , IStringLocalizer<ProductsController> localizer
            , IStringLocalizerFactory stringLocalizerFactory
            )
        {
            _IProductsService = iProductsService;

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            else
                _logger = logger.ForContext<ProductsController>();

            _sharedLocalizer = sharedLocalizer;
            _localizer = localizer;

            var assemblyName = typeof(ProductsController).GetTypeInfo().Assembly.GetName().Name;
            var stringLocalizer = stringLocalizerFactory.Create("SharedResource", assemblyName);
            _stringLocalizerFactory = stringLocalizer;
        }



        /// <summary>取得所有的Product資料
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public ActionResult<ResultModel> List()
        {
            //api/Products
            try
            {
                _logger.Debug("取得產品列表");

                //List相當於mvc的index  api/Products
                var result = new ResultModel
                {
                    Data = _IProductsService.GetAll().Result,
                    IsSuccess = true
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "");
                return BadRequest(new ResultModel { IsSuccess = false, Message = "" });
                throw;
            }
        }

        /// <summary>取得傳入編號，產品詳細資訊
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public ActionResult<ResultModel> Get(int id)
        {
            //api/Products/75

            try
            {
                _logger.Debug("取得產品資訊：{id}", id);


                var result = new ResultModel
                {
                    Data = _IProductsService.Get(id).Result,
                    IsSuccess = true
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "");
                return BadRequest(new ResultModel { IsSuccess = false, Message = "" });
                throw;
            }
        }


        [HttpGet]
        [Route("Testlanguage")]
        public ActionResult<ResultModel> Testlanguage()
        {
            //api/Products/Content
            try
            {
                _logger.Debug("測試取得語系1：" + CultureInfo.CurrentCulture.Name);
                _logger.Debug("測試取得語系2：" + _localizer["abc"]);
                _logger.Debug("測試取得語系3：" + _sharedLocalizer["abc"]);
                _logger.Debug("測試取得語系4：" + _stringLocalizerFactory["abc"]);


                var result = new ResultModel
                {
                    Data = $"CurrentCulture: {CultureInfo.CurrentCulture.Name}\r\n"
                 + $"CurrentUICulture: {CultureInfo.CurrentUICulture.Name}\r\n"
                 + $"{_localizer["abc"]}\r\n"
                 + $"{_sharedLocalizer["abc"]}\r\n"
                 + $"{_stringLocalizerFactory["abc"]}",
                    IsSuccess = true
                };

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.Error(e, "");
                return BadRequest(new ResultModel { IsSuccess = false, Message = "" });
                throw;
            }
        }





    }

}
