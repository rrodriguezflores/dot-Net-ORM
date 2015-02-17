using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using rirf.orm.Dao.Attributes;

namespace rirf.orm.Dao
{
    public class LogicEntity : Entity
    {
        /// <summary>
        /// Esconde el atributo isDelete de su super clase Entity para permitir agregar 
        /// el mapeo de la propiedad que indica el borrado l[ogic del registro en BD.
        /// </summary>
        [Column("N_ELIMINADO", DbType.Int32)]
        protected Int32 isDelete;

        /// <summary>
        /// Asigna o devuelve una bandera que indica si el objeto ah sido marcado como eliminado
        /// </summary>
        public override bool IsDelete
        {
            get { return isDelete == 1 ? true : false; } 
            set { isDelete = value ? 1 : 0; }
        }
    }
}
