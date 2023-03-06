using System;
using System.Collections.Generic;

namespace ProyectoSonia.Models;

public partial class Informe
{
    public int IdInformes { get; set; }

    public DateTime? Fecha { get; set; }

    public DateTime? FechaInicial { get; set; }

    public DateTime? FechaFinal { get; set; }

    public sbyte? LinkGenerated { get; set; }
}
