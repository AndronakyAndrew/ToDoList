using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class TodoController : Controller
    {
        private readonly TodoContext db;

        public TodoController(TodoContext context)
        {
            db = context;
        }

        //Метод для отображения задач
        public async Task<IActionResult> Index(bool? showCompleted)
        {
            IQueryable<TodoItem> todos = db.TodoItems;

            var todoItems = db.TodoItems.AsQueryable();

            if (showCompleted.HasValue)
            {
                if (showCompleted.Value)
                {
                    todoItems = todoItems.Where(item => item.IsCompleted);
                }
                else
                {
                    todoItems = todoItems.Where(item => !item.IsCompleted);
                }
            }
            ViewBag.ShowCompleted = showCompleted.HasValue ? showCompleted.Value.ToString() : "";

            return View(todoItems.ToList());
        }

        //Метод для отображения формы задачи
        public IActionResult Create()
        {
            return View();
        }

        //Метод для создания задачи
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TodoItem item)
        {
            if(ModelState.IsValid)
            {
                db.Add(item);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        //Отображение формы редактирования
        public async Task<IActionResult> Edit(int id)
        {
            var todoItem = await db.TodoItems.FindAsync(id);
            if(todoItem == null)
            {
                return NotFound(); //Отправляем 404, если задача не найдена
            }
            return View(todoItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TodoItem item)
        {
            if(id != item.Id)
            {
                return NotFound(); //Отправляем 404, если id не соответствует Id задачи
            }

            if(ModelState.IsValid)
            {
                try
                {
                    db.Update(item);
                    await db.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException)
                {
                    if(!TodoItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var todoItem = await db.TodoItems.FindAsync(id);
            if(todoItem == null)
            {
                return NotFound();
            }
            return View(todoItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todoItem = await db.TodoItems.FindAsync(id);
            db.TodoItems.Remove(todoItem);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoItemExists(int id)
        {
            return db.TodoItems.Any(e => e.Id == id);
        }

        public async Task<IActionResult> ChangeStatus(int id)
        {
            var todoItem = await db.TodoItems.FindAsync(id);
            if(todoItem == null)
            {
                return NotFound();
            }
            todoItem.IsCompleted = !todoItem.IsCompleted;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
