using DSS_MVC.Models;
using DSS_MVC.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSS_MVC.Repository
{
    public class BookRepository : IBook
    {
        private DBContext db;
        private IConfiguration _configuration;
        public BookRepository(DBContext _db, IConfiguration configuration)
        {
            db = _db;
            _configuration = configuration;
        }

        public IEnumerable<Book> GetBooks => db.Books;

        public void Add(Book book, IFormFile photo)
        {
            if (book.BookId == 0)
            {
                db.Books.Add(book);
                db.SaveChanges();

                if (photo != null)
                {
                    string imagesPath = _configuration.GetValue<string>("PaintingPhotosLocation");

                    int newImageIndex = 0;
                    //Image lastImage = _context.Images.Where(a => a.PaintingId == painting.PaintingId).OrderBy(a => a.Index).LastOrDefault();
                    //if (lastImage != null)
                    //{
                    //    newImageIndex = lastImage.Index++;
                    //}

                    string directoryPath = Path.Combine(imagesPath, book.BookId.ToString());
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    string fileName = string.Format("{0}.jpg", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                    string filePath = Path.Combine(directoryPath, fileName);

                    //try
                    //{
                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            photo.CopyTo(fileStream);
                        }

                        Image image = new Image();
                        image.BookId = book.BookId;
                        image.Index = newImageIndex;
                        image.FileName = fileName;
                        image.Name = book.BookName;

                        //db.Books.Add(_Book);
                        db.Images.Add(image);
                        
                        db.SaveChanges();
                        //db.Books.Add(image);
                        //db.SaveChanges();

                    //}
                    //catch
                    //{
                    //    //rollback
                    //    RedirectToAction("Index");
                    //}
                }
                //db.Books.Add(_Book);
                //db.SaveChanges();
        }
            else
            {
                var dbEntity = db.Books.Find(book.BookId);
                dbEntity.BookName = book.BookName;
                dbEntity.ISBN = book.ISBN;
                //dbEntity.Images = book.Images;

                db.SaveChanges();
            }
}

        //private void RedirectToAction(string v)
        //{
        //    throw new NotImplementedException();
        //}

        public Book GetBook(int? ID)
        {
            return db.Books.Include(e => e.Loans)
                .ThenInclude(a => a.Borrowers)
                .Include(s => s.Images)
                .SingleOrDefault(a => a.BookId == ID);
        }

        public void Remove(int? ID)
        {
            Book dbEntity = db.Books.Find(ID);
            db.Books.Remove(dbEntity);
            db.SaveChanges();
        }
    }
}
