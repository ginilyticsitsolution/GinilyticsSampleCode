<?php

namespace App;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Support\Facades\Auth;
use Storage;

class InvoicesAndAttachment extends Model
{
	protected $appends = ['attachment_view_path', 'attachment_download_path'];

	/*
	@Purpose: This function is querying the database table to get the Invoice by Facility Id & Year
	*/
	public function getByFacilityIdAndYear($facilityId, $year)
	{
		return $this->where('facility_id', $facilityId)->where('year', $year)->get()->groupBy('month');
	}

	/*
	@Purpose: This function is querying the database table to get Invoice by Facility Id & Month and Year
	*/
	public function getByFacilityIdAndMonth($facilityId, $year, $month)
	{
		return $this->where('facility_id', $facilityId)->where('year', $year)->where('month', $month)->get()->groupBy('month');
	}

	/*
	@Purpose: This function is inserting a new Invoice in the database table  
	*/
	public function insertAttachments($request)
	{
		$user = Auth::user();
		$month = $request->input('selectedMonth');
		$year = $request->input('selectedYear');
		$facilityId = $user->facilityData->facility_id;
		if($request->total_files > 0) {   
			for ($x = 0; $x < $request->total_files; $x++) {
				//write code here to save the uploaded files as attachment
			}
		}
		return $this->getByFacilityIdAndYear($facilityId, $year);
	}

	/*
	@Purpose: This function is setting a new key named "attachment_view_path" in the model object. It is  being used for path public path of Invoice.
	*/
	public function getAttachmentViewPathAttribute()
	{
		$url = config('attachments.invoices_attachment_folder_path');
		$url = str_replace('{facility_id}', $this->facility_id, $url);
		$url = str_replace('{year}', $this->year, $url);
		$url = str_replace('{month}', $this->month, $url);
		$url = $url . $this->attachment;
		return $url;
	}

	/*
	@Purpose: This function is setting a new key named "attachment_download_path" in the model object and generating a proper url. This url can be used to download the Invoice. 
	*/
	public function getAttachmentDownloadPathAttribute()
	{
		$url = config('attachments.invoices_attachment_folder_path');
		$url = str_replace('{facility_id}', $this->facility_id, $url);
		$url = str_replace('{year}', $this->year, $url);
		$url = str_replace('{month}', $this->month, $url);
		$url = $url . $this->attachment;
		return $url;
	}

	/*
	@Purpose: This function is used to delete the Invoice identified by an Id and also destroying the file from file system also.
	*/
	public function deleteById($id)
	{
		$attachment = $this->find($id);
		$attachment_view_path = str_replace('storage/', '', $attachment->attachment_view_path);
		unlink(Storage::disk('public')->path($attachment_view_path));
		return $this->destroy($id);
	}

	/*
	@Purpose: This function is used to update the name of Invoice identified by an Id
	*/
	public function findAndUpdateName($data)
	{
		$attachment = $this->find($data['id']);
		$attachment->name = $data['name'] . '.' . $attachment->file_extension;
		$attachment->save();
	}
}