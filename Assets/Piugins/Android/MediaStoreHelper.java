package com.yourcompany.mediastore;

import android.content.ContentResolver;
import android.database.Cursor;
import android.net.Uri;
import android.provider.MediaStore;
import com.unity3d.player.UnityPlayer;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Locale;

public class MediaStoreHelper {
    
    public static String getImagesByDate(String targetDate) {
        // targetDate 형식: "2025-11-14"
        ArrayList<String> imagePaths = new ArrayList<>();
        
        ContentResolver resolver = UnityPlayer.currentActivity.getContentResolver();
        Uri collection = MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
        
        String[] projection = {
            MediaStore.Images.Media._ID,
            MediaStore.Images.Media.DATA,
            MediaStore.Images.Media.DATE_TAKEN
        };
        
        String selection = MediaStore.Images.Media.DATE_TAKEN + " >= ? AND " +
                          MediaStore.Images.Media.DATE_TAKEN + " < ?";
        
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.US);
            Date date = sdf.parse(targetDate);
            long startTime = date.getTime();
            long endTime = startTime + (24 * 60 * 60 * 1000); // +1일
            
            String[] selectionArgs = {
                String.valueOf(startTime),
                String.valueOf(endTime)
            };
            
            Cursor cursor = resolver.query(collection, projection, selection, selectionArgs, 
                                          MediaStore.Images.Media.DATE_TAKEN + " DESC");
            
            if (cursor != null) {
                while (cursor.moveToNext()) {
                    String path = cursor.getString(
                        cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA));
                    imagePaths.add(path);
                }
                cursor.close();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        
        // JSON 형식으로 반환
        return String.join("|", imagePaths);
    }
}