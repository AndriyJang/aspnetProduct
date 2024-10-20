using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Data.Entities;
using WebApplication2.Interfaces;
using WebApplication2.Models.Category;

namespace WebApplication2.Controllers
{
    public class MainController : Controller
    {
        private readonly AndriyContexDb _dbContext;
        private readonly IImageWorker _imageWorker;
        //Зберігає різну інформацію про MVC проект
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper;
        //DI - Depencecy Injection
        public MainController(AndriyContexDb context,
            IWebHostEnvironment environment, IImageWorker imageWorker,IMapper mapper)
        {
            _dbContext = context;
            _environment = environment;
            _imageWorker = imageWorker;
            _mapper = mapper;
        }

        //метод у контролері називаться - action - дія
        public IActionResult Index()
        {
            List<CategoryItemViewModel> model = _dbContext.Categories
                .ProjectTo<CategoryItemViewModel>(_mapper.ConfigurationProvider)
                .ToList();
            return View(model);
        }

        [HttpGet] //це означає, що буде відображатися сторінки для перегляду
        public IActionResult Create()
        {
            //Ми повертає View - пусту, яка відобраєате сторінку де потрібно ввести дані для категорії
            return View();
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            var model = new CategoryCreateViewModel
            {
                Name = category.Name,
                Description = category.Description                
            };

            return View(model);
        }



        [HttpPost] //це означає, що ми отримуємо дані із форми від клієнта
        public IActionResult Create(CategoryCreateViewModel model)
        {
            var entity = _mapper.Map<CategoryEntity>(model);
            //Збережння в Базу даних інформації
            var dirName = "uploading";
            var dirSave = Path.Combine(_environment.WebRootPath, dirName);
            if (!Directory.Exists(dirSave))
            {
                Directory.CreateDirectory(dirSave);
            }
            if (model.Photo != null)
            {
                //унікальне значенн, яке ніколи не повториться
                //string fileName = Guid.NewGuid().ToString();
                //var ext = Path.GetExtension(model.Photo.FileName);
                //fileName += ext;
                //var saveFile = Path.Combine(dirSave, fileName);
                //using (var stream = new FileStream(saveFile, FileMode.Create)) 
                //    model.Photo.CopyTo(stream);
                entity.Image = _imageWorker.Save(model.Photo);
            }
            
            _dbContext.Categories.Add(entity);
            _dbContext.SaveChanges();
            //Переходимо до списку усіх категорій, тобото визиваємо метод Index нашого контролера
            return Redirect("/");
        }
        [HttpPost]
        public IActionResult Edit(CategoryCreateViewModel model, int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = model.Name;
            category.Description = model.Description;

            // Обробляємо оновлення зображення лише якщо було завантажено нове
            if (model.Photo != null)
            {
                // Зберігаємо нове зображення
                var newImageName = _imageWorker.Save(model.Photo);

                // Якщо раніше було зображення, видаляємо його
                if (!string.IsNullOrEmpty(category.Image))
                {
                    _imageWorker.Delete(category.Image);
                }

                // Присвоюємо нове ім'я зображення до категорії
                category.Image = newImageName;
            }

            _dbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var category = _dbContext.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(category.Image))
            {
                _imageWorker.Delete(category.Image);
            }
            _dbContext.Categories.Remove(category);
            _dbContext.SaveChanges();

            return Json(new { text = "Ми його видалили" }); // Вертаю об'єкт у відповідь
        }
        
        
    }

}


