using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        //inyecciónes
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;


        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;

        }

        //Listar productos------------------------------------
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] //<-- Usuario no autorizado a acceder a este recurso
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDTO = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDTO);
        }


        //Listar un producto por id ------------------------------------------------------
        [HttpGet("{productId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult GetProduct(int productId)
        {
            var product = _productRepository.GetProduct(productId);
            if (product == null)
            {
                return NotFound($"El producto con el {productId} no existe");
            }
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }




        //crear Producto------------------------------------------------------------------------
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]  

        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            //entrada correcta?
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }

            //Existe product?

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "El producto ya existe");
                return BadRequest(ModelState);
            }

            //verificar si la categoria es valida
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La categoría con el id {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            //DTo a entidad
            var product = _mapper.Map<Product>(createProductDto);

            //si no se crea
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CursomError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }

            //el retorno
            var createProduct = _productRepository.GetProduct(product.ProductId);
            var productoDTO = _mapper.Map<ProductDto>(createProduct);
            return CreatedAtRoute("GetProduct", new {productId = product.ProductId}, productoDTO);



        }








    }
}
