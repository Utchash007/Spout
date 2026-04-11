using Microsoft.EntityFrameworkCore;
using Twit.Models;
using Twit.Models.ViewModels;
using Twit.UnitOfWork;

namespace Twit.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Post> CreatePost(string userProfileId, string content)
        {
            var post = new Post
            {
                UserProfileId = userProfileId,
                Content = content
            };

            await _unitOfWork.PostRepo.Add(post);
            return post;
        }

        public async Task<Post> EditPost(string postId, string content)
        {
            var post = await _unitOfWork.PostRepo.Get(postId);
            post.Content = content;
            await _unitOfWork.PostRepo.Update(post);
            return post;
        }

        public async Task<Post> RePost(string userProfileId, string parentPostId)
        {
            var originalPost = await _unitOfWork.PostRepo.Get(parentPostId);

            var repost = new Post
            {
                UserProfileId = userProfileId,
                IsRepost = true,
                ParentPostId = parentPostId,
                Content = originalPost.Content
            };

            await _unitOfWork.PostRepo.Add(repost);

            originalPost.RepostCount++;
            await _unitOfWork.PostRepo.Update(originalPost);

            return repost;
        }

        public async Task<IEnumerable<PostViewModel>> FetchPosts()
        {
            return await _unitOfWork.PostRepo.GetAll()
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Content = p.Content,
                    LikesCount = p.LikesCount,
                    RepostCount = p.RepostCount,
                    IsRepost = p.IsRepost,
                    CreatedAt = p.CreatedAt,
                    AuthorName = p.UserProfile.FirstName + " " + p.UserProfile.LastName,
                    AuthorHandle = p.UserProfile.User.UserName ?? "",
                    AuthorInitials = (p.UserProfile.FirstName.Length > 0 ? p.UserProfile.FirstName.Substring(0, 1) : "?")
                                   + (p.UserProfile.LastName.Length > 0 ? p.UserProfile.LastName.Substring(0, 1) : "?")
                })
                .ToListAsync();
        }

        public async Task DeletePost(string postId)
        {
            await _unitOfWork.PostRepo.Delete(postId);
        }
    }
}
