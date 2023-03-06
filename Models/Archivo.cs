using System;
using System.Collections.Generic;

namespace ProyectoSonia.Models;

public partial class Archivo
{
    public int IdArchivos { get; set; }

    public int? IdInformes { get; set; }

    public string? Link { get; set; }

    public string? Nombre { get; set; }
}
