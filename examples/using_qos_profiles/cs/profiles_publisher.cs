/*******************************************************************************
 (c) 2005-2014 Copyright, Real-Time Innovations, Inc.  All rights reserved.
 RTI grants Licensee a license to use, modify, compile, and create derivative
 works of the Software.  Licensee has the right to distribute object form only
 for use with RTI products.  The Software is provided "as is", with no warranty
 of any type, including any warranty for fitness for any purpose. RTI is under
 no obligation to maintain or support the Software.  RTI shall not be liable for
 any incidental or consequential damages arising out of the use or inability to
 use the software.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
/* profiles_publisher.cs

   A publication of data of type profiles

   This file is derived from code automatically generated by the rtiddsgen 
   command:

   rtiddsgen -language C# -example <arch> profiles.idl

   Example publication of type profiles automatically generated by 
   'rtiddsgen'. To test them follow these steps:

   (1) Compile this file and the example subscription.

   (2) Start the subscription with the command
       objs\<arch>\profiles_subscriber <domain_id> <sample_count>
                
   (3) Start the publication with the command
       objs\<arch>\profiles_publisher <domain_id> <sample_count>

   (4) [Optional] Specify the list of discovery initial peers and 
       multicast receive addresses via an environment variable or a file 
       (in the current working directory) called NDDS_DISCOVERY_PEERS. 

   You can run any number of publishers and subscribers programs, and can 
   add and remove them dynamically from the domain.


   Example:

       To run the example application on domain <domain_id>:

       bin\<Debug|Release>\profiles_publisher <domain_id> <sample_count>
       bin\<Debug|Release>\profiles_subscriber <domain_id> <sample_count>

       
modification history
------------ -------       
*/

public class profilesPublisher {

    public static void Main(string[] args) {

        // --- Get domain ID --- //
        int domain_id = 0;
        if (args.Length >= 1) {
            domain_id = Int32.Parse(args[0]);
        }

        // --- Get max loop count; 0 means infinite loop  --- //
        int sample_count = 0;
        if (args.Length >= 2) {
            sample_count = Int32.Parse(args[1]);
        }

        /* Uncomment this to turn on additional logging
        NDDS.ConfigLogger.get_instance().set_verbosity_by_category(
            NDDS.LogCategory.NDDS_CONFIG_LOG_CATEGORY_API, 
            NDDS.LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
        */
    
        // --- Run --- //
        try {
            profilesPublisher.publish(
                domain_id, sample_count);
        }
        catch(DDS.Exception)
        {
            Console.WriteLine("error in publisher");
        }
    }

    static void publish(int domain_id, int sample_count) {
        /* There are several different approaches for loading QoS profiles from
         * XML files (see Configuring QoS with XML chapter in the RTI Connext 
         * Core Libraries and Utilities User's Manual). In this example we 
         * illustrate two of them:
         *
         * 1) Creating a file named USER_QOS_PROFILES.xml, which is loaded,
         * automatically by the DomainParticipantFactory. In this case, the file
         * defines a QoS profile named volatile_profile that configures reliable,
         * volatile DataWriters and DataReaders.
         *
         * 2) Adding XML documents to the DomainParticipantFactory using its
         * Profile QoSPolicy (DDS Extension). In this case, we add
         * my_custom_qos_profiles.xml to the url_profile sequence, which stores
         * the URLs of all the XML documents with QoS policies that are loaded 
         * by the DomainParticipantFactory aside from the ones that are 
         * automatically loaded.
         * my_custom_qos_profiles.xml defines a QoS profile named
         * transient_local_profile that configures reliable, transient local
         * DataWriters and DataReaders.
         */

        /* To load my_custom_qos_profiles.xml, as explained above, we need to 
         * modify the  DDSTheParticipantFactory Profile QoSPolicy */

        DDS.DomainParticipantFactoryQos factory_qos = 
            new DDS.DomainParticipantFactoryQos();
        DDS.DomainParticipantFactory.get_instance().get_qos(factory_qos);

        /* We are only going to add one XML file to the url_profile sequence,
         * so we ensure a length of 1,1. */
        factory_qos.profile.url_profile.ensure_length(1, 1);

        /* The XML file will be loaded from the working directory. That means, 
         * you need to run the example like this:
         * ./objs/<architecture>/profiles_publisher
         * (see README.txt for more information on how to run the example).
         *
         * Note that you can specify the absolute path of the XML QoS file to 
         * avoid this problem.
         */

        factory_qos.profile.url_profile.set_at(0, 
            "file://my_custom_qos_profiles.xml");
        DDS.DomainParticipantFactory.get_instance().set_qos(factory_qos);

        
        // --- Create participant --- //

        /* Our default Qos profile, volatile_profile, sets the participant name.
         * This is the only participant_qos policy that we change in our
         * example. As this is done in the default QoS profile, we don't need
         * to specify its name, so we can create the participant using the
         * create_participant() method rather than using
         * create_participant_with_profile().  */
        DDS.DomainParticipant participant =
            DDS.DomainParticipantFactory.get_instance().create_participant(
                domain_id,
                DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT, 
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
        if (participant == null) {
            shutdown(participant);
            throw new ApplicationException("create_participant error");
        }

        // --- Create publisher --- //

        /* We haven't changed the publisher_qos in any of QoS profiles we use in
         * this example, so we can just use the create_publisher() method. If 
         * you want to load an specific profile in which you may have changed 
         * the publisher_qos, use the create_publisher_with_profile() method. */
        DDS.Publisher publisher = participant.create_publisher(
                DDS.DomainParticipant.PUBLISHER_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
        if (publisher == null) {
            shutdown(participant);
            throw new ApplicationException("create_publisher error");
        }

        // --- Create topic --- //

        /* Register type before creating topic */
        System.String type_name = profilesTypeSupport.get_type_name();
        try {
            profilesTypeSupport.register_type(
                participant, type_name);
        }
        catch(DDS.Exception e) {
            Console.WriteLine("register_type error {0}", e);
            shutdown(participant);
            throw e;
        }

        /* We haven't changed the topic_qos in any of QoS profiles we use in 
         * this example, so we can just use the create_topic() method. If you 
         * want to load an specific profile in which you may have changed the 
         * topic_qos, use the create_topic_with_profile() method. */
        DDS.Topic topic = participant.create_topic(
            "Example profiles",
            type_name,
            DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (topic == null) {
            shutdown(participant);
            throw new ApplicationException("create_topic error");
        }

        // --- Create writers --- //

        /* Volatile writer -- As volatile_profile is the default qos profile
         * we don't need to specify the profile we are going to use, we can
         * just call create_datawriter passing DDS_DATAWRITER_QOS_DEFAULT. */
        DDS.DataWriter writer_volatile = publisher.create_datawriter(
            topic,
            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (writer_volatile == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_datawriter error");
        }

        /* Transient Local writer -- In this case we use
         * create_datawriter_with_profile, because we have to use a profile
         * other than the default one. This profile has been defined in
         * my_custom_qos_profiles.xml, but since we already loaded the XML file
         * we don't need to specify anything else. */

        DDS.DataWriter writer_transient_local = publisher.create_datawriter(
            topic,
            DDS.Publisher.DATAWRITER_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
        if (writer_transient_local == null)
        {
            shutdown(participant);
            throw new ApplicationException("create_datawriter error");
        }

        profilesDataWriter profiles_writer_volatile =
            (profilesDataWriter)writer_volatile;
        
        profilesDataWriter profiles_writer_transient_local =
            (profilesDataWriter)writer_transient_local;

        // --- Write --- //

        /* Create data sample for writing */
        profiles instance = profilesTypeSupport.create_data();
        if (instance == null) {
            shutdown(participant);
            throw new ApplicationException(
                "profilesTypeSupport.create_data error");
        }

        /* For a data type that has a key, if the same instance is going to be
           written multiple times, initialize the key here
           and register the keyed instance prior to writing */
        DDS.InstanceHandle_t instance_handle = DDS.InstanceHandle_t.HANDLE_NIL;
        /*
        instance_handle = profiles_writer.register_instance(instance);
        */

        /* Main loop */
        const System.Int32 send_period = 1000; // milliseconds
        for (int count=0;
             (sample_count == 0) || (count < sample_count);
             ++count) {
            Console.WriteLine("Writing profiles, count {0}", count);

            /* Modify the data to be sent here */
            instance.profile_name = "volatile_profile";
            instance.x = count;

            Console.WriteLine("Writing profile_name = " + instance.profile_name
                        + "\t x = " + instance.x);
            try {
                profiles_writer_volatile.write(instance, ref instance_handle);
            }
            catch(DDS.Exception e) {
                Console.WriteLine("write volatile error {0}", e);
            }

            instance.profile_name = "transient_local_profile";
            instance.x = count;

            Console.WriteLine("Writing profile_name = " + instance.profile_name
                        + "\t x = " + instance.x + "\n");

            try
            {
                profiles_writer_transient_local.write(instance, 
                    ref instance_handle);
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("write transient local error {0}", e);
            }

            System.Threading.Thread.Sleep(send_period);
        }

        /*
        try {
            profiles_writer.unregister_instance(
                instance, ref instance_handle);
        } catch(DDS.Exception e) {
            Console.WriteLine("unregister instance error: {0}", e);
        }
        */

        // --- Shutdown --- //

        /* Delete data sample */
        try {
            profilesTypeSupport.delete_data(instance);
        } catch(DDS.Exception e) {
            Console.WriteLine(
                "profilesTypeSupport.delete_data error: {0}", e);
        }

        /* Delete all entities */
        shutdown(participant);
    }

    static void shutdown(
        DDS.DomainParticipant participant) {

        /* Delete all entities */

        if (participant != null) {
            participant.delete_contained_entities();
            DDS.DomainParticipantFactory.get_instance().delete_participant(
                ref participant);
        }

        /* RTI Connext provides finalize_instance() method on
           domain participant factory for people who want to release memory
           used by the participant factory. Uncomment the following block of
           code for clean destruction of the singleton. */
        /*
        try {
            DDS.DomainParticipantFactory.finalize_instance();
        } catch (DDS.Exception e) {
            Console.WriteLine("finalize_instance error: {0}", e);
            throw e;
        }
        */
    }
}

