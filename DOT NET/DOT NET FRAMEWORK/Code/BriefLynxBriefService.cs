using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Com.Wolfgang.Amadeus.Diagnostics2;
using BriefLynxNew.BriefLynx.Common.BriefService.Models;
using BriefLynxNew.BriefLynx.Common.Files;
using BriefLynxNew.BriefLynx.Common.Interfaces.Files;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services.Files;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services.Linking;
using BriefLynxNew.BriefLynx.Common.Interfaces.Services.Shares;
using BriefLynxNew.BriefLynx.Common.PDF;
using BriefLynxNew.BriefLynx.DataAccess;
using BriefLynxNew.BriefLynx.DataAccess.Enumerations;
using BriefLynxNew.BriefLynx.DataAccess.Exceptions;
using BriefLynxNew.BriefLynx.DataAccess.Exceptions.Briefs;
using BriefLynxNew.BriefLynx.DataAccess.Extensions;
using BriefLynxNew.BriefLynx.DataAccess.Interfaces;
using BriefLynxNew.BriefLynx.DataAccess.Model;

namespace BriefLynxNew.BriefLynx.Common.Briefs
{
	public class BriefLynxBriefService : IBriefLynxBriefService
	{
		private readonly IBriefRepository _briefRepository;
		private readonly IBriefLynxDocumentRepository _documentRepository;
		private readonly IRemoteFileRepository _remoteFileRepository;
		private readonly IBriefReviewRepository _briefReviewRepository;
		private readonly IDocumentService _documentService;
		private readonly IZipFileService _zipFileService;
		private readonly IDocumentLinkRepository _documentLinkRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ILinkPatternExpressionRepository _linkPatternRepository;
		private readonly ITempPathProvider _tempPathProvider;
		private readonly IDocumentPlaceholderService _documentPlaceholderService;
		private const string TraceCategory = "BriefService";

		public BriefLynxBriefService(IBriefRepository briefRepository, IDocumentService documentService, IDocumentLinkingService linkingService,
		                             IBriefLynxDocumentRepository documentRepository, IRemoteFileRepository remoteFileRepository,
		                             IBriefReviewRepository briefReviewRepository, IZipFileService zipFileService, ILocalFileStorageService localFileService,
		                             IShareService shareService, IDocumentLinkRepository documentLinkRepository, IActivityRepository activityRepository,
		                             ILinkPatternExpressionRepository linkPatternRepository, ITempPathProvider tempPathProvider, IDocumentPlaceholderService documentPlaceholderService)
		{
			if (briefRepository == null)
				throw new ArgumentNullException("briefRepository");
			if (documentService == null)
				throw new ArgumentNullException("documentService");
			if (linkingService == null)
				throw new ArgumentNullException("linkingService");
			if (documentRepository == null)
				throw new ArgumentNullException("documentRepository");
			if (remoteFileRepository == null)
				throw new ArgumentNullException("remoteFileRepository");
			if (briefReviewRepository == null)
				throw new ArgumentNullException("briefReviewRepository");
			if (localFileService == null)
				throw new ArgumentNullException("localFileService");
			if (zipFileService == null)
				throw new ArgumentNullException("zipFileService");
			if (shareService == null)
				throw new ArgumentNullException("shareService");
			if (documentLinkRepository == null)
				throw new ArgumentNullException("documentLinkRepository");
			if (activityRepository == null)
				throw new ArgumentNullException("activityRepository");
			if (linkPatternRepository == null)
				throw new ArgumentNullException("linkPatternRepository");
			if (tempPathProvider == null)
				throw new ArgumentNullException("tempPathProvider");
			if (documentPlaceholderService == null)
				throw new ArgumentNullException("documentPlaceholderService");

			_briefRepository = briefRepository;
			_documentService = documentService;
			_documentRepository = documentRepository;
			_remoteFileRepository = remoteFileRepository;
			_briefReviewRepository = briefReviewRepository;
			_zipFileService = zipFileService;
			_documentLinkRepository = documentLinkRepository;
			_activityRepository = activityRepository;
			_linkPatternRepository = linkPatternRepository;
			_tempPathProvider = tempPathProvider;
			_documentPlaceholderService = documentPlaceholderService;
		}

		public bool IsReadOnly(Brief brief)
		{
			// ReSharper disable ReplaceWithStringIsNullOrEmpty
			//can't use String.IsNullOrEmpty here because it won't translate to a SQL Queary
			return brief.PassPhrase != null && brief.PassPhrase != String.Empty;
			// ReSharper restore ReplaceWithStringIsNullOrEmpty
		}

		public IQueryable<Brief> ListBriefs(User user)
		{
			if (user.UserRoles.Single().Role.IsMaster)
			{
				return _briefRepository.ListEnabledBriefs();
			}

			if (user.UserRoles.Any(ur => ur.RoleID == (int)RoleEnumeration.FirmAdministrator))
			{
				return
					_briefRepository.ListEnabledBriefs().Where(
						brief => ((brief.CreatorUserID == user.UserID) || (user.Firm == brief.Creator.Firm)));
			}

			return _briefRepository.ListEnabledBriefs().Where(brief => brief.CreatorUserID == user.UserID);
		}

		public IQueryable<Brief> ListReadOnlyBriefs(int userId)
		{
			return _briefRepository.ListReadOnlyBriefs(userId);
		}

		public BriefReview GetBriefReview(int briefReviewId)
		{
			return _briefReviewRepository.GetReview(briefReviewId);
		}

		public void DeleteMasterDocument(Brief brief)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			BriefDocument masterDocument = brief.MasterDocument;

			if (masterDocument == null)
			{
				throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Brief {0} has no master document to delete.", brief.BriefID));
			}

			EnsureNotReadOnly(brief);

			_documentService.Delete(masterDocument);

			DeleteGeneratedPackage(brief);

			CommitAllChanges();
		}

		private static void EnsureNotReadOnly(Brief brief)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			if (brief.IsReadOnly)
			{
				throw new BriefLynxServiceException("Brief is Read Only and cannot be modified");
			}
		}

		public IQueryable<BriefReview> ListBriefReviews(Brief brief)
		{
			return _briefReviewRepository.ListAll().Where(br => br.Brief == brief);
		}

		public BriefReview CreateBriefReview(Brief brief, User reviewer)
		{
			return _briefRepository.NewBriefReview(brief, reviewer);
		}

		public IQueryable<Brief> ListAllBriefs()
		{
			return _briefRepository.ListAll();
		}

		public IQueryable<BriefReview> ListBriefReviews(User reviewer)
		{
			if (reviewer == null)
				throw new ArgumentNullException("reviewer");

			return _briefReviewRepository.ListAll().Where(br => br.Reviewer == reviewer);
		}

		public Brief GetBrief(int briefId)
		{
			return _briefRepository.GetBrief(briefId);
		}

		public PackageStatusEnumeration GetBriefPackageStatus(int briefId)
		{
			return _briefRepository.GetBriefPackageStatus(briefId);
		}

		public void AddCachedThumbnailsDocument(CachedThumbnailImagesDocumentModel cachedThumbnailImagesDocumentModel)
		{
			var parentDocument = _documentService.GetDocument<BriefDocument>(cachedThumbnailImagesDocumentModel.ParentDocumentId);

			if (parentDocument == null)
			{
				throw new BriefLynxServiceException(
					"The document with ID {0} could not be found.".FormatInvariant(cachedThumbnailImagesDocumentModel.ParentDocumentId));
			}

			var document = AddBriefDocument(cachedThumbnailImagesDocumentModel);

			_documentRepository.DeleteAll(parentDocument.CachedThumbnailImageDocuments.AsQueryable());
				// make sure there aren't any other thumbnail image documents

			parentDocument.CachedThumbnailImageDocuments.Add(document);

			_briefRepository.CommitAllChanges();
		}


		public BriefDocument AddBriefDocument(BriefDocumentModel briefDocumentModel)
		{
			if (briefDocumentModel == null)
				throw new ArgumentNullException("briefDocumentModel");

			var brief = GetBrief(briefDocumentModel.BriefId);

			if (brief == null)
				throw new BriefLynxServiceException("The specified Brief does not exist.");

			using (var transaction = _briefRepository.BeginTransaction())
			{
				BriefDocument document = null;

				try
				{
					document = _documentService.CreateBriefDocument(brief, briefDocumentModel);

					brief.BriefDocuments.Add(document);

					_briefRepository.CommitAllChanges(); //this saves the document as well

					transaction.Commit();

					return document;
				}
				catch (SqlException ex)
				{
					RollbackDocumentInsert(brief, transaction, document);

					var errorMessages = ex.Errors.OfType<SqlError>().Select(e => e.Message).Aggregate(string.Empty,
					                                                                               (seed, err) =>
					                                                                               "{0}{1}{2}".FormatInvariant(seed,
					                                                                                                           !string.IsNullOrEmpty(seed)
					                                                                                                           	? " -- "
					                                                                                                           	: string.Empty, err));

					Exception docCreationException;

					if (errorMessages.Contains("Cannot insert duplicate key row in object 'dbo.tblDocument' with unique index 'IXU_tblDocument_zBriefIDzFileName'"))
					{
						docCreationException = new DuplicateDocumentNameException("A document already exists with this file name.",
						                                                          new List<string> {document != null ? document.FileName : "NULL DOCUMENT"}, ex);
					}
					else if (errorMessages.Contains("deadlock")) //TODO: is this right?
					{
						docCreationException = new DocumentCreationException("A master document already exists for this brief. Your document has not been saved. Please refresh the page.", ex);
					}
					else
					{
						docCreationException =
							new DocumentCreationException("An unexpected error occurred while creating the Document : {0}".FormatInvariant(ex.Message), ex);
					}

					AcgTrace.WriteError("Error creating document for Brief {0}".FormatInvariant(brief.BriefID), TraceCategory, ex);

					throw docCreationException;
				}
				catch (UniqueMasterDocumentException ex)
				{
					RollbackDocumentInsert(brief, transaction, document);

					brief.BriefDocuments.Remove(document);

					AcgTrace.WriteError("Unique Master Document Exception Encountered while creating Master Document for Brief {0}".FormatInvariant(brief.BriefID),
					                    TraceCategory, ex);

					throw;
				}
				catch (Exception ex)
				{
					RollbackDocumentInsert(brief, transaction, document);

					string error = "Unhandled exception encountered creating document {0} for BriefID {1}".FormatInvariant(briefDocumentModel.FileName, brief.BriefID);

					AcgTrace.WriteError(error, TraceCategory, ex);

					throw;
				}
			}
		}

		private void RollbackDocumentInsert(Brief brief, IDbTransaction transaction, BriefDocument document)
		{
			transaction.Rollback(); //rollback SQL transaction

			//wipe document from data context and file storage
			if (document != null && brief.BriefDocuments.Contains(document))
			{
				brief.BriefDocuments.Remove(document);
			}
			_documentService.Delete(document);
		}

		private void CheckForDuplicateMasterDocuments(Brief brief)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			var masterDocuments = brief.BriefDocuments.Where(d => d.DocumentTypeID == (int) DocumentTypeEnumeration.Master).ToList();

			if (masterDocuments.Count() > 1)
			{
				try
				{
					//attempt to delete the last added doc
					_documentService.Delete(masterDocuments.OrderByDescending(b => b.BriefID).First());
				}
				catch (InvalidOperationException ex)
				{
					if (ex.Message.Contains("Sequence contains more than one element"))
						throw new UniqueMasterDocumentException("A master document already exists for this brief; your document was not saved. Please refresh the page.");
				}
			}
		}

		/// <summary>
		/// Updates the last modified date for the brief.
		/// </summary>
		/// <param name="brief">The brief to mark as recently modified.</param>
		public void UpdateLastModified(Brief brief)
		{
			brief.LastModifiedDate = DateTime.Now;

			_briefRepository.CommitAllChanges();
		}

		public Folder<BriefDocumentModel> GetBriefFolderStructure(int briefId, string rootFolderName, bool addIndex)
		{
			var masterDocuments = _briefRepository.ListBriefMasterDocuments(briefId).ToList();
			var allDocuments = _briefRepository.ListBriefValidDestinationDocuments(briefId);

			// If there is supposed to be an index included in the package, add an index image if there is one.
			if (addIndex)
			{
				masterDocuments.AddRange(_briefRepository.ListBriefImageDocuments(briefId));
			}

			return CreateFolders(briefId, rootFolderName, masterDocuments, allDocuments);
		}

		/// <summary>
		/// Gets the brief structure.
		/// </summary>
		/// <param name="briefId">The brief id.</param>
		/// <param name="addIndex">if set to <c>true</c> [add index].</param>
		/// <returns>
		/// A Folder object containing the Brief Folder Structure.
		/// </returns>
		public Folder<BriefDocumentModel> GetBriefFolderStructure(int briefId, bool addIndex)
		{
			//String.Empty is the root folder, set to string.empty from requirements
			return GetBriefFolderStructure(briefId, String.Empty, addIndex);
		}

		public IQueryable<Brief> ListBriefs(Firm firm)
		{
			if (firm == null)
				throw new ArgumentNullException("firm");
			return _briefRepository.ListAll().Where(b => b.Creator.Firm == firm);
		}

		public IQueryable<BriefDocument> ListBriefValidDestinationDocuments(int briefId)
		{
			return _briefRepository.ListBriefValidDestinationDocuments(briefId);
		}

		public IQueryable<AllExhibitByMatterList> GetExhibitListByMatter(User user, int matterId)
		{
			return _briefRepository.GetExhibitListByMatter(user,matterId);
		}

		/// <summary>
		/// Creates the actual folders structure object.
		/// </summary>
		/// <param name="briefId"> </param>
		/// <param name="folderName">Name of the curretn folder.</param>
		/// <param name="mainDocuments">The main documents in the current folder.</param>
		/// <param name="subDocuments">The sub documents that will need to have sub-folders created.</param>
		/// <returns></returns>
		private Folder<BriefDocumentModel> CreateFolders(int briefId, string folderName, IList<BriefDocument> mainDocuments,
		                                                 IQueryable<BriefDocument> subDocuments)
		{
			var folders = new List<Folder<BriefDocumentModel>>();

			// Add the main document to this top level folder.
			var documents = mainDocuments;
			var childDocuments = (subDocuments ?? Enumerable.Empty<BriefDocument>()).ToList();

			var referencedBriefs = childDocuments.Where(d => d.BriefID != briefId);
				//these are Master Documents from Another Brief, which are referenced by the current brief

			//update all non-master docs to use the short version of their file names,
			//master document package file names will be un-altered unless modified by the user preference
			foreach (var doc in documents)
			{
				if (String.IsNullOrEmpty(doc.PackageFileName))
				{
					//TODO: Implement short file names for placeholders, which will involve modifying the way the 
					//package validator finds/replaces placeholder files 
					doc.PackageFileName =
						(doc.DocumentTypeID == (int) DocumentTypeEnumeration.Master || doc.DocumentUploadStatusEnum == DocumentUploadStatusEnumeration.Unmanaged
						 	? doc.FileName
						 	: doc.FileName.ShortFileName()).FileFriendlyName();
				}
			}

			//create an exhibits folder for all exhibit documents, excluding referenced brief documents
			folders.Add(new Folder<BriefDocumentModel>("Exhibits", childDocuments.Where(d => d.BriefID == briefId).Select(BriefDocumentModel.FromEntity),
			                                           Enumerable.Empty<Folder<BriefDocumentModel>>()));

			//empty folder that will contain modified exhibit documents (for annotated link destinations)
			folders.Add(new Folder<BriefDocumentModel>("LinkDestinations", Enumerable.Empty<BriefDocumentModel>(),
			                                           Enumerable.Empty<Folder<BriefDocumentModel>>()));

			//create an external docs folder for external links
			folders.Add(new Folder<BriefDocumentModel>("ExternalDocs", Enumerable.Empty<BriefDocumentModel>(),
			                                           Enumerable.Empty<Folder<BriefDocumentModel>>()));

			var referencedBriefFolders =
				referencedBriefs.Select(
					masterDoc =>
					new Folder<BriefDocumentModel>(masterDoc.Brief.Name, new[] {BriefDocumentModel.FromEntity(masterDoc)},
					                               Enumerable.Empty<Folder<BriefDocumentModel>>())).ToList();

			//create referenced brief folder containing a subfolder per referenced brief
			folders.Add(new Folder<BriefDocumentModel>("ReferencedBriefs", Enumerable.Empty<BriefDocumentModel>(), referencedBriefFolders));

			// Return the newly created folder with document and sub-folders/documents.
			return new Folder<BriefDocumentModel>(folderName.DirectoryNameSafe(), documents.Select(BriefDocumentModel.FromEntity), folders);
		}

		/// <summary>
		/// Deletes the brief index image.
		/// </summary>
		/// <param name="briefID">The brief ID.</param>
		public void DeleteBriefIndexImage(int briefID)
		{
			_documentService.DeleteDocuments(_briefRepository.ListBriefImageDocuments(briefID).Cast<Document>());
		}

		public void DeleteGeneratedPackage(Brief brief)
		{
			if (brief != null && (!String.IsNullOrEmpty(brief.ZipFileKey) || brief.ZipFileCreationStatusID != (int) PackageStatusEnumeration.NotGenerated))
			{
				if (!String.IsNullOrEmpty(brief.ZipFileKey))
				{
					try
					{
					    _remoteFileRepository.DeleteFile(brief.ZipFileUri);
					}
                    catch (RepositoryFileNotFoundException ex)
					{
					    AcgTrace.WriteWarning("Attempted to delete generated package but the file did not exist. Continuing Anyhow...", TraceCategory, ex);
					}

					brief.ZipFileKey = null;
					brief.ZipFileSize = null;
				}

				if (brief.ZipFileCreationStatusID != (int) PackageStatusEnumeration.Aborted)
				{
					brief.ZipFileCreationStatusID = (int) PackageStatusEnumeration.NotGenerated;
				}

				var annotatedMasterDocuments =
					_documentRepository.ListDocuments().OfType<BriefDocument>().Where(
						d => d.Brief == brief && d.DocumentTypeID == (int) DocumentTypeEnumeration.AnnotatedMaster).Cast<Document>();

				if (annotatedMasterDocuments.Any())
				{
					_documentService.DeleteDocuments(annotatedMasterDocuments.AsQueryable());
				}

				if (brief.PdfPortfolio != null)
				{
					_documentService.DeleteDocuments(new[] {(Document) brief.PdfPortfolio}.AsQueryable());
				}

				if (brief.IpadCompatibleBriefDocument != null)
				{
					_documentService.DeleteDocuments(new[] {(Document) brief.IpadCompatibleBriefDocument}.AsQueryable());
				}

				_documentRepository.CommitAllChanges();
			}
		}

		/// <summary>
		/// Adds the brief PDF portfolio to the Brief.
		/// </summary>
		/// <param name="briefId">The brief id.</param>
		/// <param name="packageTransformation">The PDF portfolio.</param>
		/// <param name="portfolioType">The pdf portfolio type</param>
		public void AddBriefPackageTransformation(int briefId, IBriefLynxFile packageTransformation, BriefPackageTransformationType portfolioType)
		{
			if (packageTransformation == null)
				throw new ArgumentNullException("packageTransformation");

			var brief = GetBrief(briefId);

			//Determine proper portfolio doc type
			DocumentTypeEnumeration docType = portfolioType == BriefPackageTransformationType.PdfPortfolio
			                                  	? DocumentTypeEnumeration.PDFPortfolio
			                                  	: DocumentTypeEnumeration.IpadCompatiblePDFPortfolio;

			//CreateDocuments(briefId, docType, Enumerable.Repeat(pdfPortfolio, 1), true);

			Uri fileUri = _remoteFileRepository.SaveFile(packageTransformation);

			var briefDocumentName = brief.MasterDocument.PackageFileName ?? brief.MasterDocument.FileName;

			string documentFileName = "{0}_{1}{2}".FormatInvariant(Path.GetFileNameWithoutExtension(briefDocumentName), docType.GetDescription(),
			                                                       Path.GetExtension(briefDocumentName));

			AddBriefDocument(new BriefDocumentModel(briefId, documentFileName, fileUri)
			                 	{
			                 		BriefName = brief.Name,
			                 		DocumentType = docType,
			                 		DocumentFormatType = DocumentFormatTypeEnumeration.PDF,
			                 		DocumentSection = DocumentSection.BriefDocument,
			                 		PageCount = _documentService.GetPageCount(packageTransformation),
			                 		UploadStatus = DocumentUploadStatusEnumeration.Complete,
			                 		FileLengthKb = (long?) (packageTransformation.FileSize*1024),
			                 		LastModified = DateTime.Now
			                 	});
		}

		/// <summary>
		/// Loads the brief package structure.
		/// </summary>
		/// <param name="extractedPackagePath"> </param>
		/// <returns></returns>
		public Folder<PackagedDocument> LoadBriefPackageStructure(string extractedPackagePath)
		{
			if (!Directory.Exists(extractedPackagePath))
			{
				throw new ArgumentException("Path must exist and be the root directory of an extracted Brief Package", "extractedPackagePath");
			}

			string manifestXml;

			string manifestFilePath = Path.Combine(extractedPackagePath, ManifestFactory.ManifestFileName);

			if (!File.Exists(manifestFilePath))
			{
				throw new FileNotFoundException(
					"Could not find the Manifest file to load package file strucure.  The searched path was {0}".FormatInvariant(manifestFilePath));
			}

			using (var manifestReader = File.OpenText(manifestFilePath))
			{
				manifestXml = manifestReader.ReadToEnd();
			}

			var manifestDoc = new XmlDocument();
			manifestDoc.LoadXml(manifestXml);

			var documentElements = manifestDoc.GetElementsByTagName("Document");
			var placeholderElements = manifestDoc.GetElementsByTagName("Placeholder");

            



			var documents = new List<PackagedDocument>();
			var folders = new List<Folder<PackagedDocument>>();

			//master doc && index
			var manifestNode = manifestDoc.GetElementsByTagName("Manifest")[0];


            //Get breif document link(s) info for iPad Lite version ------------

            var briefIDNode = manifestNode.Attributes["briefid"];
            if (briefIDNode == null)
            {
                throw new FormatException("iPadLite.Bad Manifest ID Node Format!");
            }

            var briefId = int.Parse(briefIDNode.Value);
            var brief = GetBrief(briefId);
            
            if (brief == null)
            {
                throw new FormatException("iPadLite.Missing brief ID!");
            }

            if (brief.MasterDocument == null) //Master document is the brief.
            {
                throw new FormatException("iPadLite.Missing Master Document!");
            }











			bool hasIndex = false;
			if (manifestNode.Attributes["hasindex"] != null)
			{
				hasIndex = Boolean.Parse(manifestNode.Attributes["hasindex"].Value);
			}

			if (hasIndex)
			{
				documents.Add(new PackagedDocument(-1, PackagedDocumentTypeEnumeration.Index, "index.pdf", "application/pdf"));
			}

			foreach (XmlNode docElement in documentElements)
			{
                AddDocumentAndFolder(docElement, folders, documents, brief);
			}

			foreach (XmlNode phElement in placeholderElements)
			{
                AddDocumentAndFolder(phElement, folders, documents, brief);
			}

			return new Folder<PackagedDocument>(string.Empty, documents, folders);
		}

        /// <summary>
        /// Adds the document and folder.
        /// </summary>
        /// <param name="manifestNode">The manifest node.</param>
        /// <param name="folders">The folders.</param>
        /// <param name="rootDocuments">The root documents.</param>
        /// <param name="brief">The brief.</param>
        /// <param name="parentBrief">The parent brief.</param>
		private static void AddDocumentAndFolder(XmlNode manifestNode, ICollection<Folder<PackagedDocument>> folders,
		                                         ICollection<PackagedDocument> rootDocuments, Brief brief)
		{
			var docIdString = manifestNode.Attributes["id"].Value;
			var docType = (PackagedDocumentTypeEnumeration) docIdString[0];
			var path = manifestNode.Attributes["path"].Value;
			var checksum = manifestNode.Attributes["checksum"].Value;
			var lastModified = DateTime.Parse(manifestNode.Attributes["lastmodified"].Value, CultureInfo.CurrentCulture);
			var folderPath = Path.GetDirectoryName(path);


            var documentID = Int32.Parse(docIdString.Substring(1), CultureInfo.InvariantCulture);
            
            //get the current documents master document (this may not be the brief master document due to linked briefs)
            var documentLinks = brief.MasterDocument.SourceLinks.OfType<AttachmentLink>().Where(al => al.DestinationDocumentID == documentID).ToList();

            //Custom links supercede the normal document thus we must check and ensure the custom has not been added if it has that link is removed from out list
            // this prevents the same document from being added multiple times
            foreach (var folder in folders)
            {
                foreach (var doc in folder.Documents)
                {
                    // only custom can override normal
                    if (doc.DocumentType == PackagedDocumentTypeEnumeration.CustomDestinationDocument)
                    {
                        foreach (var link in documentLinks)
                        {
                            if (doc.Links.Any(l => l.LinkID == link.DocumentLinkID))
                            {
                                documentLinks = documentLinks.Where(l => l.DocumentLinkID != link.DocumentLinkID).ToList();
                            }
                        }
                    }
                }
            }


            var document = new PackagedDocument(documentID, docType, Path.GetFileName(path),
                                                BriefLynxFile.GetMimeType(Path.GetExtension(path)))
                            {
                                Checksum = checksum,
                                LastModified = lastModified,
                                PackageRelativeFolderPath = folderPath,
                                Links = docType == PackagedDocumentTypeEnumeration.DestinationDocument
                                || docType == PackagedDocumentTypeEnumeration.PlaceholderDocument ? documentLinks.Select(dl => new PackagedDocumentLink
                                {
                                    DestinationPage = dl.DestinationPage ?? 1,
                                    StartPage = dl.DestinationStartPage ?? 1,
                                    EndPage = dl.DestinationEndPage ?? 1,
                                    SourceStartPage = dl.SourceStartPage,
                                    PageZoom = dl.DestinationPageZoomOptionEnumeration,
                                    LinkID = dl.DocumentLinkID
                                }
                                                                             ) : Enumerable.Empty<PackagedDocumentLink>()

                            };

            if (docType == PackagedDocumentTypeEnumeration.CustomDestinationDocument)
            {
                var link = brief.MasterDocument.SourceLinks.OfType<AttachmentLink>().Where(al => al.DocumentLinkID == documentID).Single();
                
                // set the start and end pages properly 
                // start will always be 1
                // end will always be pagecount (dest-start)+1
                // dest will be dest-start +1
                // *note* pages are base 1 that is the reason for the +1 this conforms with how Destination docs work
                document.Links = new[] 
                { 
                    new PackagedDocumentLink { 
                        DestinationPage = (link.DestinationPage ?? 1) - (link.DestinationStartPage ?? 1)+1, 
                        StartPage = 1,
                        EndPage = (link.DestinationEndPage ?? 1) - (link.DestinationStartPage ?? 1)+1, 
                        SourceStartPage = link.SourceStartPage,                         
                        PageZoom = link.DestinationPageZoomOptionEnumeration,
                        LinkID = link.DocumentLinkID } 
                };

                //Custom links supercede the normal document thus we mus find this link in the original and remove it.
                // this prevents the same document from being added multiple times
                foreach (var folder in folders)
                {
                    foreach (var doc in folder.Documents)
                    {
                        if (doc.Links.Any(l => l.LinkID == link.DocumentLinkID))
                        {
                            doc.Links = doc.Links.Where(l => l.LinkID != link.DocumentLinkID);
                        }
                    }
                }
            }



			if (!string.IsNullOrEmpty(folderPath))
			{
				var folder = folders.Where(f => f.FullPath == folderPath).FirstOrDefault();
				if (folder != null)
				{
					folder.AddDocuments(new[] {document});
				}
				else
				{
					folder = new Folder<PackagedDocument>(folderPath, new[] {document}, Enumerable.Empty<Folder<PackagedDocument>>());
					folders.Add(folder);
				}
			}
			else
			{
				rootDocuments.Add(document);
			}
		}

		/// <summary>
		/// Set the annotated master document for a brief.  To be used when viewing this
		/// brief's master document as a destination document (i.e. referenced brief) in other briefs.
		/// </summary>
		/// <param name="briefId">The brief id.</param>
		/// <param name="annotatedMasterDocumentFile">Annotated version of the Master Document</param>
		public BriefDocument SetViewerAnnotatedMasterDocument(int briefId, IBriefLynxFile annotatedMasterDocumentFile)
		{
			Brief brief = GetBrief(briefId);

			if(brief.MasterDocument == null)
			{
				throw new InvalidOperationException("Cannot Create a Viewer-Capable Annotated Master Document: The Brief does not have a Master Document");
			}

			var viewerAnnotatedFile = PdfEditor.CreateDocumentViewerFriendlyAnnotatedDocument(annotatedMasterDocumentFile, _tempPathProvider);
			var viewerAnnotatedFileUri = _remoteFileRepository.SaveFile(viewerAnnotatedFile);

			//delete any previous viewer annotated master documents
			_documentService.DeleteDocuments(
				brief.BriefDocuments.Where(d => d.DocumentTypeID == (int) DocumentTypeEnumeration.AnnotatedMaster).Cast<Document>().AsQueryable());

			//Add the new Annotated Master Doucment for Document Viewing
			var viewerAnnotatedFileName =
				"{0}_annotated{1}".FormatInvariant(Path.GetFileNameWithoutExtension(brief.MasterDocument.PackageFileName ?? brief.MasterDocument.FileName),
				                                   annotatedMasterDocumentFile.FileExtension);
			return
				AddBriefDocument(new BriefDocumentModel(briefId, viewerAnnotatedFileName, viewerAnnotatedFileUri)
				                 	{
				                 		DocumentFormatType = viewerAnnotatedFile.DocumentFormatType,
				                 		DocumentType = DocumentTypeEnumeration.AnnotatedMaster,
				                 		LastModified = DateTime.Now,
				                 		PageCount = _documentService.GetPageCount(viewerAnnotatedFile),
				                 		UploadStatus = DocumentUploadStatusEnumeration.Complete,
				                 		FileLengthKb = (long?) (viewerAnnotatedFile.FileSize/1024)
				                 	});
		}

		public void DeleteBriefViewer(BriefViewer viewer)
		{
			if (viewer == null)
				throw new ArgumentNullException("viewer");

			_briefRepository.DeleteBriefViewer(viewer);
		}

		public void Refresh<TEntity>(IEnumerable<TEntity> entities, RefreshMode refreshMode) where TEntity : BriefLynxEntity
		{
			_briefRepository.Refresh(entities, refreshMode);
		}

		/// <summary>
		/// Determines if the brief's master document has failed to upload
		/// </summary>
		/// <param name="brief"> The Brief </param>
		/// <returns><c>true</c> if there are upload errors, <c>false</c> otherwise.</returns>
		public bool BriefHasMasterUploadErrors(Brief brief)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			return brief.MasterDocument == null || brief.MasterDocument.DocumentUploadStatusID != (int) DocumentUploadStatusEnumeration.Complete;
		}

		public bool BriefHasAttachmentUploadErrors(Brief brief)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");
			return brief.MasterDocument != null && brief.MasterDocument.DocumentUploadStatusEnum == DocumentUploadStatusEnumeration.Error;
		}

		public IQueryable<BriefDocument> ListBriefAttachmentDocuments(Brief brief)
		{
			if (brief == null)
			{
			    throw new ArgumentNullException("brief");
			}
			return brief.BriefDocuments.Where(d => d.DocumentTypeID == (int) DocumentTypeEnumeration.Attachment).AsQueryable();
		}

		public IQueryable<NumberedDocumentLink> ListNumberedDocumentLinks(Brief brief)
		{
			return _documentLinkRepository.ListNumberedDocumentLinks(brief.BriefID);
		}

		//TODO: Migrate these methods from old Brief Service

		public void CommitAllChanges()
		{
			_briefRepository.CommitAllChanges();
		}

		public void AddBriefDocumentPlaceholders(Brief brief, IEnumerable<IBriefLynxPlaceholderFile> filePlaceholders)
		{
			if (brief == null)
			{
			    throw new ArgumentNullException("brief");
			}
			if (filePlaceholders == null)
			{
			    throw new ArgumentNullException("filePlaceholders");
			}

			EnsureNotReadOnly(brief);

			try
			{
				// Do not upload the video file to the file storage service, specify false
				//CreateDocuments(briefID, DocumentTypeEnumeration.Attachment, briefLynxFiles, false);
				foreach (var file in filePlaceholders)
				{
					var pathToPlaceholder = _documentPlaceholderService.GetPlaceholderFile(file.FileExtension).FilePath;

					var addedDocument = AddBriefDocument(new BriefDocumentModel(brief.BriefID, file.FileName, new Uri(pathToPlaceholder))
					                 	{
					                 		DocumentType = DocumentTypeEnumeration.Attachment,
											DocumentFormatType = file.DocumentFormatType,
					                 		UploadStatus = DocumentUploadStatusEnumeration.Unmanaged,
					                 		//Important to indicate that this document has not been uploaded
					                 		LastModified = DateTime.Now,
					                 		DocumentSection = DocumentSection.BriefDocument
					                 	});

				    file.DocumentId = addedDocument.DocumentID;
				}
			}
			catch(ArgumentException ex)
			{
				if(ex.Message.Contains("Could not find placeholder file for the media type"))
				{
					throw new DocumentCreationException("Could not find an appropriate Document Placeholder File.", ex);
				}
				throw;
			}
			catch (DocumentCreationException ex)
			{
				if (ex.DocumentCreationErrorMessages != null)
				{
					var dupFileNames =
						ex.DocumentCreationErrorMessages.Where(
							dn => dn.Contains("document already exists with this file name"));
					if (dupFileNames.Any())
					{
						throw new DuplicateDocumentNameException(dupFileNames);
					}
				}
				throw;
			}
		}

		public Brief NewBrief(User briefOwner)
		{
		    if (briefOwner == null)
		    {
		        throw new ArgumentNullException("briefOwner");
		    }

		    var newBrief = _briefRepository.NewBrief(briefOwner);

			//default options
			newBrief.Options = new BriefOption
			                   	{
			                   		AddReturnButtons = true,
			                   		AddPackageIndex = false,
			                   		DefaultDocumentZoomLevelTypeID = (int) ZoomLevelTypeEnumeration.ActualSize,
			                   		DefaultPageZoomTypeID = (int) ZoomTypeEnumeration.EntireDocument
			                   	};

			return newBrief;
		}

		public void InitializeBrief(Brief brief, User briefOwner, INotificationService notificationService)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");
			if (briefOwner == null)
				throw new ArgumentNullException("briefOwner");
			if (notificationService == null)
				throw new ArgumentNullException("notificationService");

			EnableAllSiteAndFirmExpressions(brief);

			_activityRepository.NewActivity("Brief", briefOwner, brief);

			_activityRepository.CommitAllChanges();

			try
			{
				// Send a notification that a new brief has been created
				notificationService.SendNewBriefNotification(briefOwner, brief);
			}
			catch (BriefLynxServiceException ex)
			{
				AcgTrace.WriteError(
					string.Format(CultureInfo.InvariantCulture, "An error was encountered attempting to send a New Brief Notification: {0}", ex.Message),
					"Notifications", ex.InnerException ?? ex);
			}
			catch (Exception ex)
			{
				//notification is non-critical so we'll simply log failures and continue
				AcgTrace.WriteError("An unexpected error was encountered attempting to send a New Brief Notification: {0}".FormatCurrent(ex.Message),
				                    "Notifications", ex);
			}
		}

		public void EnableAllSiteAndFirmExpressions(Brief brief)
		{
			var siteAndFirmExpressions =
				//list all Site and Firm Level Expressions which have not already been enabled for this brief
				_linkPatternRepository.ListAll().Where(lpe => lpe.LinkPatternExpressionType == (int) LinkPatternExpressionType.Site && lpe.Active).Union(
					_linkPatternRepository.ListAll().Where(
						lpe =>
						lpe.LinkPatternExpressionType == (int) LinkPatternExpressionType.Firm && ((FirmLinkPatternExpression) lpe).FirmID == brief.Creator.FirmId &&
						lpe.Active)).Where(
							lpe =>
							!_linkPatternRepository.ListAll().Where(existingLpe => existingLpe.BriefEnabledLinkPatternExpressions.Any(belpe => belpe.Brief == brief)).
							 	Contains(lpe));

			var briefID = brief.BriefID;

			foreach (var lpe in siteAndFirmExpressions)
			{
				_linkPatternRepository.NewBriefEnabledLinkPatternExpression(briefID, lpe);
			}

			_linkPatternRepository.CommitAllChanges();
		}

		public RemoveReadOnlyRestrictionResult RemoveReadOnlyRestriction(Brief brief, string passPhrase)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			var masterDocument = brief.MasterDocument;

			var activelyReferencingLinks = from rb in _briefRepository.ListAll().Where(b => b.Matter == brief.Matter)
			                               select rb.BriefDocuments
			                               into referenceBriefDocs from rbd in referenceBriefDocs select rbd.SourceLinks.OfType<AttachmentLink>()
			                               into referencingLinks from rl in referencingLinks
			                               where
			                               	((BriefDocument) rl.DestinationDocument) == masterDocument && ((BriefDocument) rl.MasterDocument).Brief != brief
			                               select rl;

			if (activelyReferencingLinks.Any())
			{
				return RemoveReadOnlyRestrictionResult.FailedReferencedBrief;
			}

			if (!string.IsNullOrEmpty(brief.PassPhrase) && brief.PassPhrase.Equals(passPhrase, StringComparison.CurrentCulture))
			{
				brief.PassPhrase = null;
				CommitAllChanges();
				return RemoveReadOnlyRestrictionResult.Succeeded;
			}

			return RemoveReadOnlyRestrictionResult.FailedInvalidPassphrase;
		}

		public ApplyReadOnlyRestrictionResult AddReadOnlyRestriction(Brief brief, string passPhrase)
		{
			if (brief == null)
				throw new ArgumentNullException("brief");

			if (brief.Shares.Count > 0)
			{
				if (brief.Shares.Any(share => share.ShareStatusEnumeration == ShareStatusEnumeration.Shared))
				{
					return ApplyReadOnlyRestrictionResult.FailedOutstandingShares;
				}
			}

			if (brief.ZipFileCreationStatusID == (int) PackageStatusEnumeration.Generated)
			{
				brief.PassPhrase = passPhrase;
				CommitAllChanges();
				return ApplyReadOnlyRestrictionResult.Succeeded;
			}

			return ApplyReadOnlyRestrictionResult.FailedNoGeneratedPackage;
		}

		public long GetPDFPackageFileLength(Uri uri)
		{
			return _remoteFileRepository.GetFileLength(uri);
		}

		public IQueryable<Mecro> MecroList(int briefId)
		{
			return _briefRepository.MecroList(briefId);
		}

		public IQueryable<Sample> SampleList(int briefId)
		{
			return _briefRepository.SampleList(briefId);
		}

		public Exhibit ExhibitById(int exhibitid)
		{
			return _briefRepository.ExhibitById(exhibitid);
		}

		public IQueryable<Mecro> GlobalMecroList()
		{
			return _briefRepository.GlobalMecroList();
		}

		public IQueryable<Sample> GlobalSampleList()
		{
			return _briefRepository.GlobalSampleList();
		}
		public void SaveMecro(Mecro mecro)
		{
			_briefRepository.SaveMecro(mecro);
		}

		public void SaveSample(Sample sample)
		{
			_briefRepository.SaveSample(sample);
		}

		public void DeleteMecro(int mecroid)
		{
			_briefRepository.DeleteMecro(mecroid);
		}

		public void DeleteSample(int sampleid)
		{
			_briefRepository.DeleteSample(sampleid);
		}
		public Mecro GetMecro(int mecroid)
		{
			return _briefRepository.GetMecro(mecroid);
		}

		public Sample GetSample(int sampleid)
		{
			return _briefRepository.GetSample(sampleid);
		}
	}
}