using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DDAC_Project.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DDAC_Project.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<DDAC_ProjectUser> _userManager;
        private readonly SignInManager<DDAC_ProjectUser> _signInManager;
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<DDAC_ProjectUser> userManager,
            SignInManager<DDAC_ProjectUser> signInManager,
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _s3Client = s3Client;
            _configuration = configuration;
            _logger = logger;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Profile Image")]
            public string?  ProfileImage { get; set; }

            [Display(Name = "Image Upload")]
            public Microsoft.AspNetCore.Http.IFormFile ? ImageUpload { get; set; }

            [Required(ErrorMessage = "First name is required.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Phone(ErrorMessage = "Invalid phone number format.")]
            [Display(Name = "Phone Number")]
            public string ? PhoneNumber { get; set; }
        }

        private async Task LoadAsync(DDAC_ProjectUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                ProfileImage = user.ProfileImage,
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }

            _logger.LogInformation($"Form file count: {Request.Form.Files.Count}");

            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files[0];
                _logger.LogInformation($"File name: {file.FileName}, File size: {file.Length}");

                if (file != null && file.Length > 0)
                {
                    try
                    {
                        var imageKey = await UploadImageToS3(file);
                        user.ProfileImage = imageKey;
                        _logger.LogInformation($"Image uploaded successfully. Image key: {imageKey}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading image to S3");
                        StatusMessage = "Error uploading image. Please try again.";
                        return RedirectToPage();
                    }
                }
                else
                {
                    _logger.LogWarning("File was null or empty");
                }
            }
            else
            {
                _logger.LogInformation("No file was uploaded");
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to update user. Errors: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                StatusMessage = "Unexpected error when trying to update user profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        private async Task<string> UploadImageToS3(IFormFile file)
        {
            var bucketName = _configuration["AWS:BucketName"];
            var keyName = $"profile-images/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            _logger.LogInformation($"Attempting to upload file {file.FileName} to S3 bucket {bucketName} with key {keyName}");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = ms,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead
                };

                try
                {
                    var response = await _s3Client.PutObjectAsync(request);
                    _logger.LogInformation($"File uploaded successfully. ETag: {response.ETag}");
                }
                catch (AmazonS3Exception ex)
                {
                    _logger.LogError(ex, "Error uploading to S3: {ErrorMessage}", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error uploading to S3");
                    throw;
                }
            }

            var fileUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";
            _logger.LogInformation($"File URL: {fileUrl}");
            return fileUrl;
        }


    }
}