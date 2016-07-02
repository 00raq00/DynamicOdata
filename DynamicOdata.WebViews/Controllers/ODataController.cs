﻿using System.Net;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using DynamicOdata.Service;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace DynamicOdata.WebViews.Controllers
{
  public class OdataController : System.Web.Http.OData.ODataController
  {
    private readonly IDataService _dataService;
    private readonly EdmModel _edmModelBuilder;

    public OdataController(IDataService dataService, EdmModel edmModelBuilder)
    {
      _dataService = dataService;
      _edmModelBuilder = edmModelBuilder;
    }

    public EdmEntityObjectCollection Get()
    {
      ODataPath path = Request.ODataProperties().Path;
      var collectionType = path.EdmType as IEdmCollectionType;
      var entityType = collectionType?.ElementType.Definition as IEdmEntityType;

      var queryContext = new ODataQueryContext(_edmModelBuilder, entityType);
      var queryOptions = new ODataQueryOptions(queryContext, Request);

      // make $count works
      var oDataProperties = Request.ODataProperties();
      if (queryOptions.InlineCount != null)
      {
        oDataProperties.TotalCount = _dataService.Count(collectionType, queryOptions);
      }

      //make $select works
      if (queryOptions.SelectExpand != null)
      {
        oDataProperties.SelectExpandClause = queryOptions.SelectExpand.SelectExpandClause;
      }

      var collection = _dataService.Get(collectionType, queryOptions);

      return collection;
    }

    public IEdmEntityObject Get(string key)
    {
      ODataPath path = Request.ODataProperties().Path;
      IEdmEntityType entityType = path.EdmType as IEdmEntityType;

      var entity = _dataService.Get(key, entityType);

      // make sure return 404 if key does not exist in database
      if (entity == null)
        throw new HttpResponseException(HttpStatusCode.NotFound);

      return entity;
    }
  }
}