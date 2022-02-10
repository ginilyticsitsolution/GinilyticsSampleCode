<?php

namespace App\Http\Controllers;

use App\User;
use ZipArchive;
use App\Facility;
use App\FacilityData;
use Illuminate\Http\Request;
use App\InvoicesAndAttachment;
use Illuminate\Support\Facades\Auth;
use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Facades\Validator;

class InvoiceController extends Controller
{
	protected const START_YEAR = 2018;   // start showing the invoices starting from this year

	/*
	@Purpose: This function is used for permissions to different modules in the invoices
	*/
	function __construct()
	{
		$this->middleware('permission:invoice-view|invoice-create|invoice-rename|invoice-delete|invoice-download-all', ['only' => 'invoicesIndex']);
		$this->middleware('permission:invoice-create', ['only' => ['invoicesStore']]);
		$this->middleware('permission:invoice-rename', ['only' => ['invoicesUpdateAttachmentName', 'getInvoicesById']]);
		$this->middleware('permission:invoice-delete', ['only' => ['invoicesDeleteSingleAttachmentById']]);
		$this->middleware('permission:invoice-download-all', ['only' => ['invoicesDownloadAsZipByMonth']]);
	}

	/*
	@Purpose: This function is used to view the landing page of the Invoices
	*/
	public function invoicesIndex(Request $request)
	{
		$user = Auth::user();
		$selectedYear = $request->input('selectedYear');
		
		($request->input('facilityId')) ? $this->updateUserFacilityData($user, $request->input('facilityId')) : null;
		$data['facilityId'] = $user->facilityData->facility_id;;
		$data['facilities'] = $user->getFacilities();
		$data['yearsInDropdown'] = $this->getYearsForInvoicesDropdown();
		$data['selectedYear'] = $this->getValidYearSelection(self::START_YEAR, $data['yearsInDropdown'][0], $selectedYear);
		$data['monthsArrayToShow'] = $this->getMonthsArrayToShow($data['selectedYear']);
		$data['invoicesAndAttachments'] = $this->getFacilityInvoicesAndAttachments($user->facilityData->facility_id, $data['selectedYear']);

		return view('communication.invoices', $data);
	}

	/*
	@Purpose: This function is updating UserFacilityData for invoices
	*/
	public function updateUserFacilityData($user, $facilityId)
	{
		return $user->updateFacilityData($facilityId);
	}

	/*
	@Purpose: This function is Fetching ValidYearSelection for invoices
	*/
	public function getValidYearSelection($startFrom, $currentYear, $selectedYear)
	{
		if($selectedYear > $currentYear || $selectedYear < $startFrom)
			return $currentYear;
		else
			return $selectedYear;
	}

	/*
	@Purpose: This function is Fetching YearsForInvoicesDropdown
	*/
	public function getYearsForInvoicesDropdown()
	{
		$currentYear = date("Y");
		$yearsList = [];
		for($index = self::START_YEAR; $index <= $currentYear; $index++)
			array_push($yearsList, $index);
		arsort($yearsList);
		return array_values($yearsList);
	}

	/*
	@Purpose: This function is returning the valid months that need to be shown on web page to upload invoices based on year
	*/
	public function getMonthsArrayToShow($year)
	{
		$path = base_path('data/months.json');
		$monthsArray = json_decode(file_get_contents($path));
		$lastMonth = ($year == date('Y')) ? date('m') -1 : 12;
		$monthsArrayToShow = array_slice($monthsArray, 0, $lastMonth);
		arsort($monthsArrayToShow);
		return array_values($monthsArrayToShow);
	}

	/*
	@Purpose: This function is fetching Facility Invoices & Attachments by FacilityId and Year
	*/
	public function getFacilityInvoicesAndAttachments($faciltyId, $selectedYear)
	{
		return $invoicesAndAttachment = (new InvoicesAndAttachment())->getByFacilityIdAndYear($faciltyId, $selectedYear);
	}

	/*
	@Purpose: This function is storing Invoices
	*/
	public function invoicesStore(Request $request)
	{
		$input_data = $request->all();
		$validator = Validator::make($input_data, [
			'files.*' => 'required|max:2050|mimes:jpg,jpeg,png,bmp'
			],[
			'files.*.required' => 'Please select a file to upload',
			'files.*.max' => 'Sorry! Maximum allowed size for an image is 2MB',
			'files.*.mimes' => 'Invalid file Selection',
			]
		);

		if ($validator->fails()) {
			return $validator->validate();
		}

		try{
			(new InvoicesAndAttachment())->insertAttachments($request);
			$status = true;
			$message = 'Invoices Uploaded Successfully';
		}
		catch(\Exception $e){
			$status = false;
			$message = 'Invoices Uploaded failed, Please try Again';
		}
		return [
			'status' => $status,
			'message' => $message,
			'data' => [] 
		];
	}

	/*
	@Purpose: This function is used to create zip files of facility invoices for selected month in a year
	*/
	public function invoicesDownloadAsZipByMonth($facilityId, $year, $month)
	{
		$invoiceAttchments = (new InvoicesAndAttachment())->getByFacilityIdAndMonth($facilityId, $year, $month);
		$attachmnetPathPrefix = config('attachments.invoices_attachment_folder_path');
		$attachmnetPathPrefix = str_replace('{facility_id}', $facilityId, $attachmnetPathPrefix);
		$attachmnetPathPrefix = str_replace('{year}', $year, $attachmnetPathPrefix);
		$attachmnetPathPrefix = str_replace('{month}', $month, $attachmnetPathPrefix);
		$attachmnetPathPrefix = str_replace('storage/', '', $attachmnetPathPrefix);
		$zipFileName = $this->getMonthNameByMonthSequenceNo($month) . "_invoices.zip";
		$zip = new ZipArchive;

		if ($zip->open(public_path($zipFileName), ZipArchive::CREATE) === TRUE) {
			$files = Storage::disk('public')->allFiles($attachmnetPathPrefix);
			foreach ($files as $key => $value) {
				$fullPath = Storage::disk('public')->path($value);
				$relativeNameInZipFile = basename($fullPath);
				$zip->addFile($fullPath, $relativeNameInZipFile);
			}
			$zip->close();
		}
		return response()->download(public_path($zipFileName))->deleteFileAfterSend(true);
	}

	/*
	@Purpose:This function is fetching the MonthName by the month sequence no
	*/
	public function getMonthNameByMonthSequenceNo($monthSequence)
	{
		$path = base_path('data/months.json');
		$monthsArray = json_decode(file_get_contents($path));
		return $monthsArray[$monthSequence - 1]->name;
	}

	/*
	@Purpose:This function is deleting a single Invoice identified by an Id
	*/
	public function invoicesDeleteSingleAttachmentById(Request $request)
	{
		$validated = request()->validate([
			'id' => 'required|exists:invoices_and_attachments'
		],
		[
		 'id.required' => 'Attachment id is a required field.',
		 'id.exists' => 'This attachment doesn\'t exist anymore'
		]);
		
		$id = $request->input('id');
		(new InvoicesAndAttachment())->deleteById($id);
		return [
			'message' => 'Attachment Delete Successfully',
			'status' => true,
			'data' => []
		];
	}

	/*
	@Purpose: This function is fetching a single Invoice By Id
	*/
	public function getInvoicesById($id)
	{
		$validator = Validator::make(['id' => $id], [
			'id' => 'required|exists:invoices_and_attachments'
		],
		[
		 'id.required' => 'Attachment id is a required field.',
		 'id.exists' => 'This attachment doesn\'t exist anymore'
		]);

		if($validator->fails())
			return $validator->validate();

		return [
			'message' => 'Invoice attachment fetched successfully',
			'status' => true,
			'data' => [
				'invoiceAttachment' => (new InvoicesAndAttachment())->find($id)
			]
		];
	}

	/*
	@Purpose:This function is updating the Attachment Name in the Iinvoice
	*/
	public function invoicesUpdateAttachmentName(Request $request)
	{
		$validated = request()->validate([
			'id' => 'required|exists:invoices_and_attachments',
			'name' => 'required'
		],
		[
		 'id.required' => 'Attachment id is a required field.',
		 'id.exists' => 'This attachment doesn\'t exist anymore',
		 'name.required' => 'Attachment name is a required field.'
		]);
		
		$data = $request->input();
		(new InvoicesAndAttachment())->findAndUpdateName($data);

		return [
			'message' => 'Invoice name updated successfully',
			'status' => true,
			'data' => [
				'invoiceAttachment' => (new InvoicesAndAttachment())->find($data['id'])
			]
		];
	}
}
