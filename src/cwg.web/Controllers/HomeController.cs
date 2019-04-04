﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using cwg.web.Generators;
using cwg.web.Models;

using Microsoft.AspNetCore.Mvc;

namespace cwg.web.Controllers
{
    public class HomeController : Controller
    {
        private static List<T> GetObjects<T>()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(a => a.BaseType == typeof(T) && !a.IsAbstract);

            return types.Select(b => (T) Activator.CreateInstance(b)).ToList();
        }

        private static IEnumerable<BaseGenerator> GetGenerators()
        {
            var baseGenerators = GetObjects<BaseGenerator>();
            return baseGenerators.OrderBy(a => a.Name);
        }

        private static IEnumerable<BaseArchiveGenerator> GetArchiveGenerators()
        {
            var archiveGenerators = GetObjects<BaseArchiveGenerator>();            

            return archiveGenerators.OrderBy(a => a.Name);
        }
        
        public IActionResult Index()
        {            
            ViewBag.archivetypes = GetArchiveGenerators().Select(a => a.Name).ToList();
            ViewBag.filetypes = GetGenerators().Select(a => a.Name).ToList();            
            return View();

        }

        private BaseGenerator getGenerator(string name) => GetGenerators().FirstOrDefault(a => a.Name == name);        

        [HttpGet]
        [HttpPost]
        public IActionResult Generate(int numberToGenerate, string fileType)
        {
            var generator = getGenerator(fileType);            

            if (generator == null)
            {
                throw new Exception($"{fileType} was not found");
            }

            var (sha1, fileName) = generator.GenerateFiles(numberToGenerate);
            
            return View("Generation", new GenerationResponseModel
            {
                FileName = fileName,
                SHA1 = sha1,
                FileType = fileType
            });
        }
    }
}