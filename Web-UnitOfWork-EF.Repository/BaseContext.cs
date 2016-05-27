﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Web_UnitOfWork_EF.Repository.Mapping;

namespace Web_UnitOfWork_EF.Repository
{
    public class BaseContext<T> : DbContext where T : class 
    {
        public DbSet<T> DbSet { get; set; }

        public BaseContext() : base("")
        {
            //Caso a base de dados não tenha sido criada
            Database.SetInitializer<BaseContext<T>>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //O evento acima está implementado na classe BaseContext.cs que acabamos de criar, porém isso 
            //não é muito inteligente concorda ? Se nosso banco tiver 30 tabelas vamos fazer isso na mão ? Se fosse 50 tabelas ?
            //modelBuilder.Configurations.Add(new UserMapping());
            //modelBuilder.Configurations.Add(new RecipeMapping());

            //base.OnModelCreating(modelBuilder);







            //Para resolver essa situação que iremos criar o mapeamento 
            //dinâmico, a principal ideia é caso seja criado um novo 
            //mapeamento não será necessário alterar o evento 
            //OnModelCreating, ele será capaz de identificar um novo modelo e 
            //carregá-lo dinamicamente


            //Pega todas as classes que estão implementando a interface IMapping
            //Assim o Entity Framework é capaz de carregar os mapeamentos
            var typesToMapping = (from types in Assembly.GetExecutingAssembly().GetTypes()
                                  where types.IsClass && typeof(IMapping).IsAssignableFrom(types)
                                  select types).ToList();



            // Com ajuda do Reflection criamos as instancias 
            // e adicionamos no Entity Framework
            foreach (var mapping in typesToMapping)
            {
                dynamic mappingClass = Activator.CreateInstance(mapping);
                modelBuilder.Configurations.Add(mappingClass);
            }

            


        }

        public virtual void ChangeObjectState(Object model, EntityState state)
        {
            //facilita quando temos alterações e exclusões
            ((IObjectContextAdapter) this).ObjectContext.ObjectStateManager.ChangeObjectState(model, state);
        }
    }
}