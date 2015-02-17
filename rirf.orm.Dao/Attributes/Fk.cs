using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace rirf.orm.Dao.Attributes
{
    /// <summary>
    /// Clase que indica que el atributo al que es aplicada forma parte de la llave primaria de la entidad
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class Fk : Column
    {
        /// <summary>
        /// Crea un nuevo objeto de tipo Fk
        /// </summary>
        /// <param name="dBname">Nombre de la columna en BD</param>
        /// <param name="dbType">Tipo de dato de la columna</param>
        public Fk(String dBname, DbType dbType) : base(dBname, dbType) { }
    }
}
