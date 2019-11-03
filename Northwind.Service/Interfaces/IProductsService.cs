using System.Collections.Generic;
using System.Threading.Tasks;
using Northwind.Entities.Models;

namespace Northwind.Service.Interfaces
{
    public interface IProductsService
    {
        Task<IEnumerable<Products>> GetAll();

        Task<Products> Get(int id);



    }



}
