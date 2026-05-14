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
            var posts = await _unitOfWork.PostRepo.GetAll()
                .Include(p => p.UserProfile)
                    .ThenInclude(up => up.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p =>
            {
                var firstName = p.UserProfile?.FirstName ?? "";
                var lastName  = p.UserProfile?.LastName  ?? "";
                var initials  = (firstName.Length > 0 ? firstName[0].ToString() : "?")
                              + (lastName.Length  > 0 ? lastName[0].ToString()  : "");

                return new PostViewModel
                {
                    Id           = p.Id,
                    Content      = p.Content,
                    LikesCount   = p.LikesCount,
                    RepostCount  = p.RepostCount,
                    IsRepost     = p.IsRepost,
                    CreatedAt    = p.CreatedAt,
                    AuthorName   = $"{firstName} {lastName}".Trim(),
                    AuthorHandle = p.UserProfile?.User?.UserName ?? "",
                    AuthorInitials = initials
                };
            });
        }

        public async Task DeletePost(string postId)
        {
            await _unitOfWork.PostRepo.Delete(postId);
        }
    }
}
