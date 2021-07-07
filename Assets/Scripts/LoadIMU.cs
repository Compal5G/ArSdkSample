using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class LoadIMU : MonoBehaviour
{	    
	public Canvas accel;
	public Canvas gyro;
	public Canvas magn;
	    
    void Start()
    {

    }
    
    void Update()
    {
    	Vector3[] imu = new Vector3[3];
    	
    	long ts = 0;    
		if( API.xslam_get_imu_array( imu, ref ts ) ){
			//Debug.Log( imu[0] );		
			if( accel ) accel.GetComponent<ImuSensor>().Set(imu[0]);
			if( gyro )  gyro.GetComponent<ImuSensor>().Set(imu[1]);
			if( magn )  magn.GetComponent<ImuSensor>().Set(imu[2]);
		} 

        //// Test 3DOF
        //API.Orientation o = new API.Orientation();
		//if( API.xslam_get_3dof( ref o ) ){
		//	Debug.LogFormat( "3DOF: {0} {1} {2} {3}", o.quaternion.x, o.quaternion.y, o.quaternion.z, o.quaternion.w );
		//}
    }
}
