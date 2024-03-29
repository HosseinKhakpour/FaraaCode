﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeTo.Core.Statics;
using CodeTo.Core.Utilities.Extensions;
using CodeTo.Core.Utilities.Other;
using CodeTo.Core.ViewModel.Articles;
using CodeTo.DataEF.Context;
using CodeTo.Domain.Entities.Articles;
using Microsoft.EntityFrameworkCore;

namespace CodeTo.Core.Services.ArticleServices.AdminArticle
{
    public class ArticleService : IArticleService
    {
        private readonly CodeToContext _context;
        private readonly ILoggerService<ArticleService> _logger;

        public ArticleService(CodeToContext context, ILoggerService<ArticleService> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<ArticleCreateOrEditViewModel> FindAsync(long id)
        {
            var model = await _context.Articles
                .FirstOrDefaultAsync(m => m.Id == id);
            return model.ToCreateOrEditViewModel();
        }

        public async Task<bool> AddAsync(ArticleCreateOrEditViewModel vm)
        {

            try
            {
                string ArticleImageName = null;
                if (vm.ArticleImageFile != null)
                {
                    ArticleImageName = GeneratorGuid.GeneratorUniqCode() + vm.ArticleImageFile.FileName;
                    var thumbSize = new ThumbSize(100, 100);
                    vm.ArticleImageFile.AddImageToServer(ArticleImageName, ArticlePathTools.ArticleImageServerPath, thumbSize,
                        vm.ArticleImageName);
                }

                _context.Articles.Add(new Article
                {
                    Id = vm.Id,
                   
                    Writer = vm.Writer,
                    ArticleTitle = vm.ArticleTitle,
                    ArticleDescription = vm.ArticleDescription,
                    ShortDescription=vm.ShortDescription,
                    CreateDate = DateTime.Now,
                    ArticleImageName = ArticleImageName,
                    IsDeleted = false
                });

                await _context.SaveChangesAsync();
                return true;

            }
            catch (Exception rx)
            {

                return false;
            }
        }

        public async Task<bool> EditAsync(ArticleCreateOrEditViewModel vm)
        {
            var article = await _context.Articles.FindAsync(vm.Id);
            try
            {
                if (vm.ArticleImageFile != null)
                {
                    var articleImageName = DateTime.Now.ToString("MM-dd-yyyy_") + vm.ArticleImageFile.FileName;
                    var thumbSize = new ThumbSize(100, 100);
                    vm.ArticleImageFile.AddImageToServer(articleImageName, ArticlePathTools.ArticleImageServerPath,
                        thumbSize, vm.ArticleImageName);
                    article.ArticleImageName = articleImageName;
                }


                article.Writer = vm.Writer;
                article.ArticleTitle = vm.ArticleTitle;
                article.ArticleDescription = vm.ArticleDescription;
                article.ShortDescription = vm.ShortDescription;
                article.LastModifyDate = DateTime.Now;

                _context.Articles.Update(article);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            try
            {
                var product = await _context.Articles.FindAsync(id);
                if (product.ArticleImageName != null)
                {
                    product.ArticleImageName.DeleteImage(ArticlePathTools.ArticleImageServerPath);
                }
                _context.Articles.Remove(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError("خطای حذف مطلب" + e.Message);
                return false;
            }
        }

        public async Task<List<ArticleIndexViewModel>> GetAllAsync()
        {
            return await _context.Articles
                .OrderByDescending(c => c.Id)
                .ToIndexViewModel()
                .ToListAsync();
        }

        public async Task<bool> IsExist(long id)
        {
            return await _context.Articles.AnyAsync(p => p.Id == id);
        }
    }
}