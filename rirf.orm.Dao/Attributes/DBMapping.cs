using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rirf.orm.Dao.Attributes
{
    /// <summary>
    /// Clase que es utilizada para establecer como entidad de BD una clase
    /// </summary>
    public class DBMapping : System.Attribute
    {
        /// <summary>
        /// El nombre de la entidad en BD
        /// </summary>
        private String dbName;

        /// <summary>
        /// Crea un nuevo objeto de tipo DBMapping
        /// </summary>
        /// <param name="dbName">Nombre de la entidad en BD</param>
        public DBMapping(String dbName)
        {
            this.dbName = dbName;
        }

        /// <summary>
        /// Devuelve el nombre de la entidad en BD
        /// </summary>
        public String DbName{
            get { return dbName; }
        }
    }
}
