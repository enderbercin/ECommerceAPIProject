using ECommerceProject.API.DataAccess;
using ECommerceProject.API.Entities;
using ECommerceProject.Core.Controllers;
using ECommerceProject.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace ECommerceProject.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]

    public class CategoryController : ControllerBase
    {
        private DataBaseContext _db;
        public CategoryController(DataBaseContext dataBaseContext)

        {
            _db = dataBaseContext;

        }

       


        [HttpPost("create")]
        [ProducesResponseType(200, Type = typeof(Resp<ApplymentAccountResponseModel>))]
        [ProducesResponseType(400, Type = typeof(Resp<ApplymentAccountResponseModel>))]
        public IActionResult Create([FromBody] CategoryCreateModel model)
        {
            Resp<CategoryModel> response = new Resp<CategoryModel>();
            string categoryName = model.Name?.Trim().ToLower();
            if (_db.Categories.Any(p => p.Name.ToLower() == categoryName))
            {
                response.AddError(nameof(model.Name), "Bu kategori adı zaten mevcuttur.");
                return BadRequest(response);
            }
            else
            {
                Category category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,

                };
                _db.Categories.Add(category);
                _db.SaveChanges();

                CategoryModel data = new CategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };
                response.Data = data;
                return Ok(data);
            }

        }
        [HttpGet("list")]
        [ProducesResponseType(200, Type = typeof(Resp<List<CategoryModel>>))]
        public IActionResult List()
        {
            Resp<List<CategoryModel>> response = new Resp<List<CategoryModel>>();
            List<CategoryModel> list = _db.Categories.Select(
               c => new CategoryModel { Id = c.Id, Name = c.Name, Description = c.Name })
                .ToList();

            response.Data = list;


            return Ok(response);
        }

        [HttpGet("getcategory/{id}")]
        [ProducesResponseType(200, Type = typeof(Resp<CategoryModel>))]
        [ProducesResponseType(404, Type = typeof(Resp<CategoryModel>))]
        public IActionResult GetCategoryById([FromRoute] int id)
        {
            Resp<CategoryModel> response = new Resp<CategoryModel>();

            Category category = _db.Categories.SingleOrDefault(x => x.Id == id);
            CategoryModel data = null;

            if (category == null) return NotFound(response);

            data = new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            response.Data = data;
            return Ok(response);
        }

        [HttpPut("update/{id}")]
        [ProducesResponseType(200, Type = typeof(Resp<CategoryModel>))]
        [ProducesResponseType(400, Type = typeof(Resp<CategoryModel>))]
        [ProducesResponseType(404, Type = typeof(Resp<CategoryModel>))]

        public IActionResult Update([FromRoute]int id ,[FromBody]CategoryCreateUpdateModel model)
        {

            Resp<CategoryModel> response = new Resp<CategoryModel>();
            Category category = _db.Categories.Find(id);

            if(category==null) 
                return NotFound(response);
            
            string categoryName= model.Name?.Trim().ToLower();
            if(_db.Categories.Any(x => x.Name.ToLower() == categoryName && x.Id!=id))
            {
                response.AddError(nameof(model.Name), "Bu kategori adı zaten mevcut.");
                return BadRequest(response);
            }
           
            category.Name = model.Name;
            category.Description = model.Description;

            _db.SaveChanges();
            CategoryModel data = new CategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
            response.Data = data;

            return Ok(response);
        }

        [HttpDelete("delete/{id}")]
        [ProducesResponseType(200, Type = typeof(Resp<CategoryModel>))]
        [ProducesResponseType(404, Type = typeof(Resp<CategoryModel>))]
        public IActionResult DeleteCategoryById([FromRoute]int id)
        {
            Resp<object> response = new Resp<object>();
            Category category = _db.Categories.Find(id);
            if (category == null) 
            {
                response.AddError(nameof(category), "Bu kategori id bulunamadı.");
                return NotFound(response); 
            }
               

            _db.Categories.Remove(category);
            _db.SaveChanges();
            return Ok(response);
        }
    }
}