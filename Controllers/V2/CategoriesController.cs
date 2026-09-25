using ApiEcommerce.Constants;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiEcommerce.Controllers.V2
{
    
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize(Roles = "admin")]
    //[EnableCors(PolicyNames.AllowSpecificOrigin)]
    public class CategoriesController : ControllerBase
    {
        
        //propiedades como Interfaces que se inyectarán
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;


        //Inyección
        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }




        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult GetCategoriesOrderById()
        {
            var categories = _categoryRepository.GetCategories().OrderBy(cat => cat.IdCategory);  
            var categoriesDto = new List<CategoryDto>();          
            foreach (var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category)); 
            }

            return Ok(categoriesDto); 
        }


        //Get Category by Id
        [AllowAnonymous]
        [HttpGet("{id:int}", Name ="GetCategory")]
        //[ResponseCache(Duration = 10)]   //segundos
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult GetCategory(int id) //match id con el de arriba
        {
            System.Console.WriteLine($"Categoría con el ID: {id} a las {DateTime.Now}");
            var category = _categoryRepository.GetCategory(id); //match name metodo conel de arriba
            System.Console.WriteLine($"Respuesta con el ID: {id}");
            if (category == null)
            {
                return NotFound($"La categoria con el id {id} no existe");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);

        }


        //POST Crear category
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]  

        public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            //verificar si es null
            if (createCategoryDto == null)
            {
                return BadRequest(ModelState);
            }

            //comprobar si ya existe
            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", "La categoría ya existe");
                return BadRequest(ModelState);
            }

            //CONVERTIR EndidadDto a EntidadCategory
            var category = _mapper.Map<Category>(createCategoryDto); //<Destino> (Origen)

            //guardarlo en la base de datos
            if (!_categoryRepository.CreateCategory(category))   
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }

            //retorar el recurso recien creado
            return CreatedAtRoute("GetCategory", new { id = category.IdCategory}, category);  //(Ruta, crearObjetoParaMostrar, Body objeto)

        }



        //Patch category
        [HttpPatch("{id:int}", Name ="UpdateCategory")      ]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto) 
        {
            //verificar si existe
            if(!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La categoria con el id {id} no existe");
            }

            //verificar si los datos vienen nullos en el body
            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }

            //verificar si existe la categoria por el name
            if(_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError","La categoría ya existe");
                return BadRequest(ModelState);
            }

            //Mapear
            var category = _mapper.Map<Category>(updateCategoryDto);
            category.IdCategory = id;

            if(!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("CurstomError", $"Algo salió mal al actualziar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }

            //return bueno
            return NoContent();
        }


        //Eliminar
        [HttpDelete("{id:int}", Name ="DeleteCategory")      ]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        public IActionResult DeleteCategory(int id) 
        {
            //verificar si existe
            if(!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"La categoria con el id {id} no existe");
            }


            //verificar si existe la categoria por el name
            var category = _categoryRepository.GetCategory(id);
            if(category == null)
            {
                return NotFound($"La categoria con el id {id} no existe");
            }



            if(!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError("CurstomError", $"Algo salió mal al eliminar el registro {category.Name}");
                return StatusCode(500, ModelState);
            }
            //return bueno
            return NoContent();
        }






    }
}
