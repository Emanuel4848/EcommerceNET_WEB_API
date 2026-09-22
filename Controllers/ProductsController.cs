using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ProductsController : ControllerBase
    {
        //inyecciónes
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;


        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;

        }

        //Listar productos------------------------------------
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        //Obtener productos por categoria------------------------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult GetProductFroCategory(int categoryId)
        {
            var products = _productRepository.GetProductForCategory(categoryId);
            if (products.Count == 0)
            {
                return NotFound($"No existen productos con la categoría {categoryId}");
            }
            var productDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDto);
        }

        //Buscar productos por nombre o descripción
        [AllowAnonymous]
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult SearchProducts(string searchTerm )
        {   
            var products = _productRepository.SearchProducts(searchTerm);
            if (products.Count == 0)
            {
                return NotFound($"Los productos con el nombre o descripción '{searchTerm}' no existen");
            }
            var productDto = _mapper.Map<List<ProductDto>>(products);

            return Ok(productDto);


        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduc")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status200OK)] 
        public IActionResult BuyProduc(string name, int quantity )
        {   
            if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
            {
                return BadRequest("El nombre del producto o la cantidad no son validos");
            }

            var foundProduct = _productRepository.ProductExists(name);
            if (!foundProduct)
            {
                return BadRequest($"El producto con el nombre '{name}' no existe");
            }

            if(!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"No se pudo comprar el producto {name} o la cantidad solicitada es mayor al stock disponible");
                return BadRequest(ModelState);
            }


            var units = quantity == 1 ? "unidad": "unidades";

            return Ok($"Se compró {quantity} {units} del producto {name}");

            
        }

        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        
        public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if(updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            
            if(!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", "El producto no existe");
                return BadRequest(ModelState);
            }

            if(!_categoryRepository.CategoryExists(updateProductDto.CategoryId)) 
            {
                ModelState.AddModelError("CustomError", $"La caregoria con el id {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            
            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId; //para que actualize y no lo cree.

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CurstomError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }


        [HttpDelete("{id:int}", Name ="DeleteProduct")      ]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] 
        [ProducesResponseType(StatusCodes.Status204NoContent)] 

        public IActionResult DeleteProduct(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ModelState);
            }


            if(!_productRepository.ProductExists(id))
            {
                return NotFound($"El producto con el {id} no existe");
            }

            var product = _productRepository.GetProduct(id);
            if(product == null)
            {
                return NotFound($"El producto con el {id} no existe");
            }

            if(!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", "Hubo un error al intentar eliminar el producto");
                return StatusCode(500, ModelState);
            }

            return NoContent();


        }




    }
}
