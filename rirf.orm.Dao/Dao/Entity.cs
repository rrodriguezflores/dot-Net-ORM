using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rirf.orm.Dao.Attributes;
using System.Data;

namespace rirf.orm.Dao
{
    /****************************************************************************************************/
    /* Clave del programa:  Entity                                                                      */
    /* Creado por: Ricardo Israel Rodriguez Flores                                                      */
    /* Fecha: 03/Diciembre/2009                                                                         */
    /* Descripción: Clase abstracta utilizada para identificar a una clase como una entidad que sera    */
    /*              mapeada a BD.                                                                       */
    /* Fecha Modificación: 14/Enero/2009                                                                */
    /* Modificado por: Ricardo Israel Rodriguez Flores                                                  */
    /* Descripción del cambio: Se agregaron banderas para determinar el estatus del objeto              */
    /****************************************************************************************************/

    /// <summary>
    /// Clase abstracta utilizada para identificar a una clase como una entidad que sera mapeada a BD.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Determina si el objeto ah sido cargado desde BD
        /// </summary>
        protected bool isFromDB;

        /// <summary>
        /// Determina si el objeto ah sido marcado como eliminado
        /// </summary>
        protected Int32 isDelete;

        /// <summary>
        /// Determina si el objeto ah sido marcado como actualizado
        /// </summary>
        protected bool isUpdate;

        /// <summary>
        /// Determina si el objeto ah sido marcado como insertado
        /// </summary>
        protected bool isInsert;

        /// <summary>
        /// Devuelve una bandera que indica si el objeto ah sido cargado desde BD
        /// </summary>
        public bool IsFromDB
        {
            get { return isFromDB; }
        }

        /// <summary>
        /// Asigna o devuelve una bandera que indica si el objeto ah sido marcado como eliminado
        /// </summary>
        public virtual bool IsDelete
        {
            get { return isDelete == 1 ? true : false; }
            set { isDelete = value ? 1 : 0; }
        }

        /// <summary>
        /// Asigna o devuelve una bandera que indica si el objeto ah sido marcado como actualizado
        /// </summary>
        public bool IsUpdate
        {
            get { return isUpdate; }
            set { isUpdate = value; }
        }

        /// <summary>
        /// Asigna o devuelve una bandera que indica si el objeto ah sido marcado como insertado
        /// </summary>
        public bool IsInsert
        {
            get { return isInsert; }
            set { isInsert = value; }
        }
    }
}
