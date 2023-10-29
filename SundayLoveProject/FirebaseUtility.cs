using Firebase.Storage;
using Microsoft.Maui.Storage;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SundayLoveProject
{
    public class FirebaseUtility

    {
        FirebaseStorage fbs;
        public static string fbName;
        public static string fbID;

        public FirebaseUtility()
        {
            //fbName = "sunday-love.appspot.com";
            //fbID = @"sunday-love/";
        }

        void Init()
        {
            if (fbs is not null)
                return;

            try {
                fbs = new FirebaseStorage(fbName);
            }
            catch (FirebaseStorageException e) { 
                Console.WriteLine("Firebase Error: unable to connect to firebase...");
            }
        }

        //save the file at filepath into the firebase storage with the filename
        public async void UploadFileAsync(string srcFilepath, string dstFilepath)
        {
            Init();
            if (fbs == null)
                return;
            var filestream = File.OpenRead(srcFilepath);
            try {
                await fbs.Child(fbID + dstFilepath).PutAsync(filestream);
            }
            catch (FirebaseStorageException e) {
                Console.WriteLine("Firebase Error: unable to upload file {0} to firebase...", srcFilepath);
            }
        }

        public async void DeleteFileAsync(string filepath)
        {
            Init();
            if (fbs == null)
                return;
            try {
                await fbs.Child(fbID + filepath).DeleteAsync();
            }
            catch (FirebaseStorageException e) {
                Console.WriteLine("Firebase Error: unable to delete file {0} from firebase...", filepath);
            }
        }

        /// <summary>
        /// Download the file and save to the local file path. 
  
        /// </summary>
        public async void DownloadFileAsync(string fbsFilepath, string localFilepath)
        {
            Init();
            if (fbs == null)
                return;
            var fileUrl = "";
            try {
                //get url
                fileUrl = await fbs.Child(fbID + fbsFilepath).GetDownloadUrlAsync();
            }
            catch (FirebaseStorageException e) {
                Console.WriteLine("Firebase Error: unable to download file {0} from firebase...", fbsFilepath);
                return;
            }
     
            using (var httpClient = new HttpClient()) {
                try {
                    // Download the file as a byte array
                    byte[] fileBytes = await httpClient.GetByteArrayAsync(fileUrl);

                    // Ensure the local directory exists
                    string directoryPath = Path.GetDirectoryName(localFilepath);
                    if (!Directory.Exists(directoryPath)) {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Save the downloaded file to the local path
                    File.WriteAllBytes(localFilepath, fileBytes);
                }
                catch (Exception ex) {
                    Console.WriteLine($"HTTP Error: Error downloading file: {ex.Message}");
                }
            }
        }

    


        public async void SyncFileAsync(string srcFilepath, string dstFilepath) {
            // Specify the file name, local file path, and remote storage path
            Init();
            if (fbs == null)
                return;

            bool fileExists = false;
            try {
                await fbs
                    .Child(fbID + dstFilepath)
                    .GetDownloadUrlAsync();
                fileExists = true;
            }
            catch (FirebaseStorageException ex) {
                Console.WriteLine("Can't find file {0} on firebase. Attempting to upload file {1} to firebase...", dstFilepath, srcFilepath);
            }
            if (!fileExists) {
                UploadFileAsync(srcFilepath, dstFilepath);
            }
        }
    }
}
