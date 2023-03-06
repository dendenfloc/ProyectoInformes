using System;
using System.Collections.Generic;

namespace ProyectoSonia.Models;

public partial class Imagene
{
    public int IdImagenes { get; set; }

    public string? Nombre { get; set; }

    public string? Link { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Medidacorrectiva { get; set; }

    public int IdInformes { get; set; }

    public int? IdAreas { get; set; }

    public string? TituloImagen { get; set; }
}
