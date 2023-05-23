using Dropbox.Api.Files;
using Dropbox.Api;
using Microsoft.AspNetCore.Mvc;
using ProyectoSonia.Models;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Dropbox.Api.Sharing;
using Dropbox.Api.TeamLog;
using Microsoft.EntityFrameworkCore;
using static Dropbox.Api.Files.SearchMatchType;
using static Dropbox.Api.TeamLog.TimeUnit;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Dropbox.Api.TeamLog.PaperDownloadFormat;
using Xceed.Words.NET;
using Xceed.Document.NET;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProyectoSonia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly SoniaproyectContext _conexion;

        private const string appKey = "rnwla4y023r4da0";
        private const string appSecret = "xz7dr4ggbtkjpe7";
        // URL de retorno después de la autenticación
        private const string redirectUri = "https://supervisioninformes.azurewebsites.net/Home/";
        private readonly IWebHostEnvironment _env;

        // Instancia del objeto DropboxClient
        private DropboxClient client;

        public ActionResult aa()
        {
            // Obtener el flujo de autenticación
            //var authorizeUrl = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, appSecret);
            //var token = authorizeUrl.ToString();
            //var auth = new DropboxOAuth2Helper(appKey, appSecret, redirectUri);

            var authUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, appKey, redirectUri, appSecret, true, false);
            //var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authUri, redirect);
            var token = authUri.ToString();
            //// Generar una URL de autenticación para el usuario
            //var authorizeUri = auth.AuthorizeUri(AuthorizationResponseType.Token);

            // Redirigir al usuario a la página de autenticación de Dropbox
            return View();
        }

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, SoniaproyectContext conexion, IWebHostEnvironment env)
        {
            _logger = logger;
            _configuration = configuration;
            _conexion = conexion;
            _env = env;

        }


        public IActionResult Index(int idInforme)
        {
            ViewBag.idInforme = idInforme;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CargarImagenesAsync(List<IFormFile> imagenes, string token, DateTime fechaI, DateTime fechaF, int idsormaae)
        {
            var modelIdinforme = 0;
            if (idsormaae == 0)
            {
                var model = new Informe();
                model.Fecha = DateTime.Now;
                model.FechaInicial = fechaI;
                model.FechaFinal = fechaF;
                _conexion.Add(model);
                await _conexion.SaveChangesAsync();

                modelIdinforme = model.IdInformes;
            }
            else
            {
                modelIdinforme = idsormaae;
            }           


            foreach (var foto in imagenes)
            {
                var nombreFoto = Path.GetFileNameWithoutExtension(foto.FileName);

                string pattern = @"(\d{4}-\d{2}-\d{2}) at (\d{1,2}.\d{2}.\d{2} [AP]M)";

                Match match = Regex.Match(nombreFoto, pattern);

                if (match.Success)
                {
                    string date = match.Groups[1].Value;
                    string time = match.Groups[2].Value;

                    // Convertir la cadena de fecha y hora a un objeto DateTime
                    DateTime fechaImagen = DateTime.ParseExact(date + " " + time, "yyyy-MM-dd h.mm.ss tt", CultureInfo.InvariantCulture);

                    try
                    {

                        using (var stream = foto.OpenReadStream())
                        {
                            var options = new DropboxClientConfig();
                            options.HttpClient = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler());
                            var accessToken = _configuration.GetSection("Dropbox")["AccessToken"];
                            var dropboxTOkenMYSQL = await _conexion.Dropboxkeys.Where(t => t.Iddropboxkey.Equals(1)).FirstOrDefaultAsync();

                            //var dbx = new DropboxClient(token, options);

                            var dbx = new DropboxClient(dropboxTOkenMYSQL.Dropboxkeycol, options);


                            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss");
                            var response = await dbx.Files.UploadAsync("/" + timestamp + ".jpg", WriteMode.Overwrite.Instance, body: stream);



                            if (response != null && response.IsFile)
                            {
                                var linkasa = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(response.PathDisplay);
                                // Obtener el enlace compartido
                                var sharedLinkaaa = linkasa.Url;

                                // Reemplazar el "dl=0" en el enlace compartido con "raw=1" para obtener el enlace de descarga directa
                                var downloadLink = sharedLinkaaa.Replace("dl=0", "raw=1");

                                var archivo = new Imagene();
                                archivo.IdImagenes = 0;
                                archivo.Nombre = response.Name;
                                archivo.Link = downloadLink;
                                archivo.Fecha = fechaImagen;                                
                                archivo.IdInformes = modelIdinforme;
                                
                                
                                _conexion.Imagenes.Add(archivo);
                                await _conexion.SaveChangesAsync();
                            }
                            else
                            {
                                return BadRequest();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Manejar la excepción aquí
                        return StatusCode(500, ex.Message);
                    }

                }
            }


       
            return RedirectToAction("formatoInforme", new { id = modelIdinforme, pageNumber = 1 });
        }
        
        public async Task<IActionResult> formatoInforme(int id, int pageNumber)
        {
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            var products = await _conexion.Imagenes.OrderBy(t=>t.Fecha).Where(t => t.IdInformes.Equals(id)).ToListAsync();
            var pageSize = 4;
            // Realizar operaciones con la lista de productos recuperados

            var ListadoNUM_ANNOS = await _conexion.Areas.ToListAsync();
            ListadoNUM_ANNOS.Add(new Area { IdAreas = 0, Nombre = " --[Seleccionar]-- " });
            ViewData["NUM_ANNOS"] = new SelectList(ListadoNUM_ANNOS.OrderBy(t => t.Nombre), "IdAreas", "Nombre");

            var PalabrasClaves = await _conexion.PalabrasClaves.ToListAsync();
            PalabrasClaves.Add(new PalabrasClave { IdPalabrasClave = 0, Nombre = " --[Seleccionar]-- " });
            ViewData["PalabrasClaves"] = new SelectList(PalabrasClaves.OrderBy(t => t.Nombre), "IdPalabrasClave", "Nombre");


            int numRegistros = 3; // Número de registros por página
            var totalImagenes = await _conexion.Imagenes.OrderBy(t => t.Fecha).Where(t => t.IdInformes.Equals(id)).CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalImagenes / numRegistros);
            var pageNumbers = Enumerable.Range(1, totalPages).ToList();
            ViewBag.paginas = pageNumbers;
            ViewBag.pageNumber = pageNumber;
            ViewBag.PreviousPageNumber = pageNumber -1 ;
            ViewBag.NextPageNumber = pageNumber +1;

            var imagenes = await _conexion.Imagenes
                                    .Where(t => t.IdInformes.Equals(id))
                                    .Skip((pageNumber - 1) * pageSize) // Omitir los registros anteriores a la página actual
                                    .Take(pageSize) // Tomar los registros de la página actual
                                    .ToListAsync();


            return View(imagenes);
        }

        public async Task<IActionResult> formatoInformeaaaa(int id, int pageNumber)
        {
            pageNumber = pageNumber == 0 ? 1 : pageNumber;
            var products = await _conexion.Imagenes.OrderBy(t => t.Fecha).Where(t => t.IdInformes.Equals(id)).ToListAsync();
            var pageSize = 4;
            // Realizar operaciones con la lista de productos recuperados

            var ListadoNUM_ANNOS = await _conexion.Areas.ToListAsync();
            ListadoNUM_ANNOS.Add(new Area { IdAreas = 0, Nombre = " --[Seleccionar]-- " });
            ViewData["NUM_ANNOS"] = new SelectList(ListadoNUM_ANNOS.OrderBy(t => t.Nombre), "IdAreas", "Nombre");

            var PalabrasClaves = await _conexion.PalabrasClaves.ToListAsync();
            PalabrasClaves.Add(new PalabrasClave { IdPalabrasClave = 0, Nombre = " --[Seleccionar]-- " });
            ViewData["PalabrasClaves"] = new SelectList(PalabrasClaves.OrderBy(t => t.Nombre), "IdPalabrasClave", "Nombre");


            int numRegistros = 3; // Número de registros por página
            var totalImagenes = await _conexion.Imagenes.OrderBy(t => t.Fecha).Where(t => t.IdInformes.Equals(id)).CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalImagenes / numRegistros);
            var pageNumbers = Enumerable.Range(1, totalPages).ToList();
            ViewBag.paginas = pageNumbers;
            ViewBag.pageNumber = pageNumber;
            ViewBag.PreviousPageNumber = pageNumber - 1;
            ViewBag.NextPageNumber = pageNumber + 1;

            var imagenes = await _conexion.Imagenes
                                    .Where(t => t.IdInformes.Equals(id))
                                    .Skip((pageNumber - 1) * pageSize) // Omitir los registros anteriores a la página actual
                                    .Take(pageSize) // Tomar los registros de la página actual
                                    .ToListAsync();


            return View(imagenes);
        }

        public JsonResult GetOptions(int value)
        {
            var descripcion = "";
            var model = _conexion.PalabrasClaveDescripcions.Where(t => t.IdPalabrasClave.Equals(value)).FirstOrDefault();
            descripcion = model == null ? "" : model.Descripcion;
            return Json(descripcion);
        }


        [HttpPost]
        public async Task<IActionResult> EnviarDatos(List<Imagene> datos)
        {
            foreach (var item in datos)
            {
                var model = await _conexion.Imagenes.Where(t => t.IdImagenes.Equals(item.IdImagenes)).FirstOrDefaultAsync();
                model.TituloImagen = item.TituloImagen == null ? "" : item.TituloImagen;
                model.Medidacorrectiva = item.Medidacorrectiva == null ? "" : item.Medidacorrectiva;
                model.Fecha = item.Fecha;
                model.IdAreas = item.IdAreas;
                _conexion.Update(model);
                await _conexion.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> GetListControl()
        {
            var model = await _conexion.Informes.ToListAsync();
            return Json(new { data = model });
        }


        public async Task<IActionResult> Listado()
        {
            var model = await _conexion.Informes.ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> ListadoArchivos(int id)
        {
            var model = await _conexion.Archivos.Where(t=>t.IdInformes.Equals(id)).ToListAsync();
            ViewBag.id = id;
            return View(model);
        }

        public async Task<IActionResult> GetListadoArchivos(int id)
        {
            var model = await _conexion.Archivos.Where(t => t.IdInformes.Equals(id)).ToListAsync();
            return Json(new { data = model });
        }


        //[HttpPost]
        public async Task<IActionResult> ProcesarWord(int id)
        {
            var imagenes = await _conexion.Imagenes.Where(t => t.IdInformes.Equals(id)).ToListAsync();
            var areas = imagenes.Select(i => i.IdAreas).Distinct().ToList();
            var areasFinal = await _conexion.Areas.Where(a => areas.Contains(a.IdAreas)).ToListAsync();
            var areasImagenes = areasFinal.Select(i => i.IdGrupoAreas).Distinct().ToList();

                foreach (var item in areasImagenes)
                {
                    var areasV1 = await _conexion.Areas.Where(i => i.IdGrupoAreas.Equals(item)).ToListAsync();
                    var areasV2 = areasV1.Select(t => t.IdAreas).ToList();
                    var imagenesV1 = imagenes.Where(a => areasV2.Contains(a.IdAreas.Value)).ToList();

                    var titulo = areasV1.FirstOrDefault();

                await GenerarDoc(imagenesV1, titulo);


            }


            var model = await _conexion.Informes.Where(t => t.IdInformes.Equals(id)).FirstOrDefaultAsync();
            model.LinkGenerated = 1;
            _conexion.Update(model);
            await _conexion.SaveChangesAsync();

            return RedirectToAction("Listado");
        }

        private async Task GenerarDoc(List<Imagene> imagenes, Area titulo)
        {
            var firstPic = imagenes.FirstOrDefault();
            var area = await _conexion.Areas.Where(t => t.IdAreas.Equals(firstPic.IdAreas)).FirstOrDefaultAsync();
            var inform = await _conexion.Informes.Where(t => t.IdInformes.Equals(firstPic.IdInformes)).FirstOrDefaultAsync();
            var rangeDate = inform.FechaInicial.Value.ToString("dd/MM/yyyy") + " al " + inform.FechaFinal.Value.ToString("dd/MM/yyyy");
            var subTittle = "Supervision de bioseguridad en " + area.TituloWord + ", del " + rangeDate;
            using (var document = DocX.Create(titulo.Nombre +".docx"))
            {
                // Inserta un párrafo con el título
                var title = document.InsertParagraph("SUPERVISIONES DE BIOSEGURIDAD");
                title.Alignment = Alignment.center;
                title.Bold();
                title.UnderlineStyle(UnderlineStyle.singleLine);

                document.InsertParagraph("\n");
                document.InsertParagraph(subTittle);
                document.InsertParagraph("\n");


                int pageSize = 3; // Número de registros por página
                var totalRecords = imagenes.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var pageNumbers = Enumerable.Range(1, totalPages).ToList();

                foreach (var item in pageNumbers)
                {
                    var products = imagenes
                                    .OrderBy(p => p.Fecha) // Ordenar por nombre (opcional)
                                    .Skip((item - 1) * pageSize) // Omitir los registros anteriores a la página actual
                                    .Take(pageSize) // Tomar los registros de la página actual
                                    .ToList();



                    foreach (var img in products)
                    {
                        // Crea una tabla de 3x3 celdas
                        Xceed.Document.NET.Table table = document.AddTable(2, 2);
                        var firstRow = table.Rows[0];
                        foreach (var cell in firstRow.Cells)
                        {
                            cell.Paragraphs[0].Bold();
                            cell.Paragraphs[0].Alignment = Alignment.center;
                            cell.FillColor = System.Drawing.Color.LightGray;
                        }
                        int numRow = 0;
                        var horsas = img.Fecha.Value.ToString("dd/MM/yyyy HH:mm");
                        var titleImagen = await _conexion.Areas.Where(t => t.IdAreas.Equals(img.IdAreas)).FirstOrDefaultAsync();

                        // Agrega contenido a la tabla
                        table.Rows[numRow].Cells[0].Paragraphs.First().InsertText(horsas + " " + titleImagen.Nombre);
                        table.Rows[numRow].Cells[1].Paragraphs.First().InsertText("MEDIDA CORRECTIVA");
                        numRow++;
                        table.Rows[numRow].Cells[0].Paragraphs.First().InsertText("\n" + img.TituloImagen + "\n");
                        table.Rows[numRow].Cells[1].Paragraphs.First().InsertText("\n" + img.Medidacorrectiva + "\r\n");
                        table.Rows[numRow].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                        table.Rows[numRow].Cells[1].Paragraphs[0].Alignment = Alignment.both;
                        table.Rows[numRow].Height = 6.0f / 2.54f * 72.0f;


                        using (var client = new WebClient())
                        {
                            var imagenBytes = client.DownloadData(img.Link);

                            // Carga la imagen en un objeto MemoryStream
                            using (var ms = new MemoryStream(imagenBytes))
                            {
                                // Agrega la imagen al documento
                                var image = document.AddImage(ms);
                                var picture = image.CreatePicture();


                                // Obtiene las dimensiones de la imagen original
                                var imageWidth = picture.Width;
                                var imageHeight = picture.Height;

                                // Calcula la proporción de aspecto de la imagen
                                var aspectRatio = (double)imageWidth / (double)imageHeight;

                                // Establece la altura deseada de la imagen en puntos (1 cm = 28.35 puntos)
                                var desiredHeight = 4.0 / 2.54 * 72.0;

                                // Calcula el ancho correspondiente manteniendo la proporción de aspecto
                                var desiredWidth = aspectRatio * desiredHeight;

                                // Ajusta el tamaño de la imagen
                                var picture2 = image.CreatePicture((int)desiredHeight, (int)desiredWidth);

                                // Obtener la celda específica de la tabla
                                var cellImagen = table.Rows[numRow].Cells[0];

                                // Agregar un párrafo con la imagen
                                var paragraph = cellImagen.Paragraphs.First();
                                var imagess = paragraph.AppendPicture(picture2);
                            }

                            numRow++;
                        }


                        document.InsertTable(table);
                    }


                    document.InsertParagraph().InsertPageBreakAfterSelf();

                }

                // Guarda el documento en un MemoryStream
                MemoryStream stream = new MemoryStream();
                document.SaveAs(stream);

                // Retorna el archivo como un File Content Result
                stream.Seek(0, SeekOrigin.Begin);

                var accessToken = _configuration.GetSection("Dropbox")["AccessToken"];
                var dropboxTOkenMYSQL = await _conexion.Dropboxkeys.Where(t => t.Iddropboxkey.Equals(1)).FirstOrDefaultAsync();
                var options = new DropboxClientConfig();
                options.HttpClient = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler());
                //var dbx = new DropboxClient("sl.BZuK1bX2YDb_Tpg5c_EV6qOzcstyom4Jt7EPSAYeysDjXA1GsEo3gOhIdnH5ryTBYwW6XkDuu4cPX1nLyZGklj8V9aFrCIWlWkajRv10eSYMKEKsPUI7ZK2X69tfwRe83w51wek14fD9", options);
                //var dbx = new DropboxClient(accessToken, options);
                var dbx = new DropboxClient(dropboxTOkenMYSQL.Dropboxkeycol, options);

                // Ruta de la carpeta que deseas crear
                var rutaCarpeta = "/" + imagenes.FirstOrDefault().IdInformes;

                try
                {
                    // Creamos la carpeta si no existe
                    var resCrearCarpeta = await dbx.Files.CreateFolderAsync(rutaCarpeta);
                }
                catch (Exception ex)
                {

                }



                var response = await dbx.Files.UploadAsync(rutaCarpeta+
                    "/" + titulo.Nombre + ".docx",
                    WriteMode.Overwrite.Instance,
                    body: stream);

                var sharedLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(rutaCarpeta +"/" + titulo.Nombre +".docx");
                var url = sharedLink.Url;
                var downloadLink = url.Replace("dl=0", "raw=1");

                var archivo = new Archivo();
                archivo.IdInformes = imagenes.FirstOrDefault().IdInformes;
                archivo.Nombre = titulo.Nombre;
                archivo.Link = downloadLink;
                _conexion.Archivos.Add(archivo);
                await _conexion.SaveChangesAsync();

                //return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "miDocumento.docx");

            }
        }


        public async Task<IActionResult> InsertarImagen(int id)
        {
            var imagenes = await _conexion.Imagenes.Where(t => t.IdInformes.Equals(31) && t.IdAreas.Equals(3)).ToListAsync();
            
            var cantImagenes = imagenes.Count();

            using (var document = DocX.Create("miDocumento.docx"))
            {
                // Inserta un párrafo con el título
                var title = document.InsertParagraph("SUPERVISIONES DE BIOSEGURIDAD");
                title.Alignment = Alignment.center;
                title.Bold();
                title.UnderlineStyle(UnderlineStyle.singleLine);

                document.InsertParagraph("\n");
                document.InsertParagraph("\n");


                int pageSize = 3; // Número de registros por página
                var totalRecords = imagenes.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                var pageNumbers = Enumerable.Range(1, totalPages).ToList();

                foreach (var item in pageNumbers)
                {
                    var products = imagenes
                                    .OrderBy(p => p.Fecha) // Ordenar por nombre (opcional)
                                    .Skip((item - 1) * pageSize) // Omitir los registros anteriores a la página actual
                                    .Take(pageSize) // Tomar los registros de la página actual
                                    .ToList();

                 

                    foreach (var img in products)
                    {
                        // Crea una tabla de 3x3 celdas
                        Xceed.Document.NET.Table table = document.AddTable(2, 2);
                        var firstRow = table.Rows[0];
                        foreach (var cell in firstRow.Cells)
                        {
                            cell.Paragraphs[0].Bold();
                            cell.Paragraphs[0].Alignment = Alignment.center;
                            cell.FillColor = System.Drawing.Color.LightGray;
                        }
                        int numRow = 0;
                        //var horsas = img.Fecha.Value.ToString(@"dd\d\ hh\h\ mm\m\ ");

                        // Agrega contenido a la tabla
                        table.Rows[numRow].Cells[0].Paragraphs.First().InsertText("21/01/2023 08:53 UCI");
                        table.Rows[numRow].Cells[1].Paragraphs.First().InsertText("MEDIDA CORRECTIVA");
                        numRow++;
                        table.Rows[numRow].Cells[0].Paragraphs.First().InsertText("\n" + img.Nombre +"\n");
                        table.Rows[numRow].Cells[1].Paragraphs.First().InsertText("\n" + img.Medidacorrectiva+"\r\n");
                        table.Rows[numRow].Cells[0].Paragraphs[0].Alignment = Alignment.center;
                        table.Rows[numRow].Cells[1].Paragraphs[0].Alignment = Alignment.both;
                        table.Rows[numRow].Height = 6.0f / 2.54f * 72.0f;
                        

                        using (var client = new WebClient())
                        {
                            var imagenBytes = client.DownloadData(img.Link);

                            // Carga la imagen en un objeto MemoryStream
                            using (var ms = new MemoryStream(imagenBytes))
                            {
                                // Agrega la imagen al documento
                                var image = document.AddImage(ms);
                                var picture = image.CreatePicture();


                                // Obtiene las dimensiones de la imagen original
                                var imageWidth = picture.Width;
                                var imageHeight = picture.Height;

                                // Calcula la proporción de aspecto de la imagen
                                var aspectRatio = (double)imageWidth / (double)imageHeight;

                                // Establece la altura deseada de la imagen en puntos (1 cm = 28.35 puntos)
                                var desiredHeight = 4.0 / 2.54 * 72.0;

                                // Calcula el ancho correspondiente manteniendo la proporción de aspecto
                                var desiredWidth = aspectRatio * desiredHeight;

                                // Ajusta el tamaño de la imagen
                                var picture2 = image.CreatePicture((int)desiredHeight, (int)desiredWidth);

                                // Obtener la celda específica de la tabla
                                var cellImagen = table.Rows[numRow].Cells[0];

                                // Agregar un párrafo con la imagen
                                var paragraph = cellImagen.Paragraphs.First();
                                var imagess = paragraph.AppendPicture(picture2);
                            }

                            numRow++;
                        }


                        document.InsertTable(table);
                    }

                    
                    document.InsertParagraph().InsertPageBreakAfterSelf();

                }
                
                // Guarda el documento en un MemoryStream
                MemoryStream stream = new MemoryStream();
                document.SaveAs(stream);

                // Retorna el archivo como un File Content Result
                stream.Seek(0, SeekOrigin.Begin);

                var options = new DropboxClientConfig();
                options.HttpClient = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler());
                var dbx = new DropboxClient("sl.BZpCjb0k55Kzr3v8Vo0skPrjUpBki9tU12u0dAwur-8pxRNb0NzXxUtaEGOFjCroDu_T1KgX_DZD_p6cSbpU4_wCWptNmgjkWq_gajqgowHTkkTNK2z1XQ90u-5CAMb7D3r4UqwK5Y42", options);


                var response = await dbx.Files.UploadAsync(
                    "/miDocumenasdtosad.docx",
                    WriteMode.Overwrite.Instance,
                    body: stream);

                var sharedLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync("/miDocumenasdtosad.docx");
                var url = sharedLink.Url;
                var downloadLink = url.Replace("dl=0", "raw=1");

                return File(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "miDocumento.docx");

            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ActualizarToken(string token)
        {
            var configuracionRoot = new ConfigurationBuilder()
                .SetBasePath(_env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            var seccion = configuracionRoot.GetSection("Dropbox");
            seccion["AccessToken"] = token;

            var archivoConfiguracion = Path.Combine(_env.ContentRootPath, "appsettings.json");
            using (var stream = new StreamWriter(archivoConfiguracion))
            {
                await stream.WriteAsync(configuracionRoot.ToString());
            }

            return Ok();
        }
    }
}