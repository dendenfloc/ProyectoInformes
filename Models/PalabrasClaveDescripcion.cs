using System;
using System.Collections.Generic;

namespace ProyectoSonia.Models;

public partial class PalabrasClaveDescripcion
{
    public int IdPalabrasClaveDescripcion { get; set; }

    public string? Descripcion { get; set; }

    public int IdPalabrasClave { get; set; }
}
