using ApiEcommerce.Models;
using ApiEcommerce.Models.DTOs;
using ApiEcommerce.Models.DTOs.Responses;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiVersionNeutral]
    public class ProductsController : ControllerBase
    {
        //inyecciónes
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        //Listar productos------------------------------------
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] //<-- Usuario no autorizado a acceder a este recurso
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDTO = products.Adapt<List<ProductDto>>();
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
            var productDto = product.Adapt<ProductDto>();
            return Ok(productDto);
        }


                //Listar un producto por id ------------------------------------------------------
        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductInPage([FromQuery] int pageNumber= 1, [FromQuery] int pageSize = 5)
        {
            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest("Los parametros de paginación no son validos");
            }
            
            var totalProducts = _productRepository.GetTotalProducts();          //50
            var totalPages = (int)Math.Ceiling((double)totalProducts/pageSize); //10
            if (pageNumber > totalPages)
            {
                return NotFound("No hay mas paginas disponibles");
            }

            var products = _productRepository.GetProductsInPage(pageNumber, pageSize);
            var productDto = products.Adapt<List<ProductDto>>();

            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productDto
            };
            return Ok(paginationResponse);
        }




        //crear Producto------------------------------------------------------------------------
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
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
            var product = createProductDto.Adapt<Product>();

            //AGREGANDO IMAGEN
            if (createProductDto.Image != null)
            {
                UploadProductImage(createProductDto, product);
                //Asignar a producto
            }
            else
            {
                //img por defecto
                product.ImgUrl = "https://placehold.co/600x400";
            }




            //si no se crea
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CursomError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }

            //el retorno
            var createProduct = _productRepository.GetProduct(product.ProductId);
            var productoDTO = createProduct.Adapt<ProductDto>();
            return CreatedAtRoute("GetProduct", new { productId = product.ProductId }, productoDTO);

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
            var productDto = products.Adapt<List<ProductDto>>();
            return Ok(productDto);
        }

        //Buscar productos por nombre o descripción
        [AllowAnonymous]
        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {
            var products = _productRepository.SearchProducts(searchTerm);
            if (products.Count == 0)
            {
                return NotFound($"Los productos con el nombre o descripción '{searchTerm}' no existen");
            }
            var productDto = products.Adapt<List<ProductDto>>();

            return Ok(productDto);


        }

        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduc")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduc(string name, int quantity)
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

            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"No se pudo comprar el producto {name} o la cantidad solicitada es mayor al stock disponible");
                return BadRequest(ModelState);
            }


            var units = quantity == 1 ? "unidad" : "unidades";

            return Ok($"Se compró {quantity} {units} del producto {name}");


        }




        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", "El producto no existe");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"La caregoria con el id {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }


            var product = updateProductDto.Adapt<Product>();
            product.ProductId = productId; //para que actualize y no lo cree.

            //AGREGANDO IMAGEN
            if (updateProductDto.Image != null)
            {
                UploadProductImage(updateProductDto, product);

                //Asignar a producto
            }
            else
            {
                //img por defecto
                product.ImgUrl = "https://placehold.co/600x400";
            }


            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CurstomError", $"Algo salió mal al guardar el registro {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();

        }

        //metodo reutilizado
        private void UploadProductImage(dynamic productDto, Product product)
        {
            // Crea un nombre único conservando la extensión (.jpg, .png...)
            string fileName = product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(productDto.Image.FileName);

            // Ruta de la carpeta: proyecto/wwwroot/ProductsImages
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");

            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            //Ruta física completa donde se guardará el archivo
            var filePath = Path.Combine(imagesFolder, fileName);


            // Si ya existe un archivo con ese nombre, lo elimina
            FileInfo file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }

            // Crea el archivo y copia dentro la imagen recibida
            using var fileStream = new FileStream(filePath, FileMode.Create);
            productDto.Image.CopyTo(fileStream);


            // Construye la dirección pública de la API
            var baseUrl =
            $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";


            //URL queusará el cliente para ver laimagen
            product.ImgUrl = $"{baseUrl}/ProductsImages/{fileName}";

            //ruta física utilizada internamente por el servior.
            product.ImgUrlLocal = filePath;
        }

        [HttpDelete("{id:int}", Name = "DeleteProduct")]
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


            if (!_productRepository.ProductExists(id))
            {
                return NotFound($"El producto con el {id} no existe");
            }

            var product = _productRepository.GetProduct(id);
            if (product == null)
            {
                return NotFound($"El producto con el {id} no existe");
            }

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", "Hubo un error al intentar eliminar el producto");
                return StatusCode(500, ModelState);
            }

            return NoContent();


        }




    }
}
