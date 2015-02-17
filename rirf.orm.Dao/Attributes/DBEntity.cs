using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rirf.orm.Dao.Attributes
{
    /// <summary>
    /// Clase que es utilizada para establecer como entidad de BD una clase
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
    public class DBEntity : DBMapping
    {
        /// <summary>
        /// Crea un nuevo objeto de tipo DBEntity
        /// </summary>
        /// <param name="dBname">Nombre de la entidad en BD</param>
        public DBEntity(String dBname) : base(dBname) { }
    }
}
