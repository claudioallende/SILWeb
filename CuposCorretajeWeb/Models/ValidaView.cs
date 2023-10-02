using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace CuposCorretajeWeb.Models
{
  public class ValidateAjaxAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (!filterContext.HttpContext.Request.IsAjaxRequest())
        return;

      var modelState = filterContext.Controller.ViewData.ModelState;
      //var errors = modelState.Where(x => x.Value.Errors.Count > 0).ToList();
      //Console.Write(errors);
      if (!modelState.IsValid)
      {
        var errorModel =
                from x in modelState.Keys
                where modelState[x].Errors.Count > 0
                select new
                {
                  key = x,
                  errors = modelState[x].Errors.
                                           Select(y => y.ErrorMessage).
                                           ToArray()
                };
        throw new Exception("La estructura del AJAX no es válida");
        //filterContext.Result = new JsonResult()
        //{
        //    Data = errorModel
        //};
        //filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      }
    }
  }
}