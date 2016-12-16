# python_tutorial_task.py - Batch Python SDK tutorial sample with cloud service configuration


from __future__ import print_function
import argparse
import collections
import os
import string

import azure.storage.blob as azureblob

if __name__ == '__main__':

    parser = argparse.ArgumentParser()
    parser.add_argument('--filepath', required=True,
                        help='The path to the text file to process. The path'
                             'may include a compute node\'s environment'
                             'variables, such as'
                             '$AZ_BATCH_NODE_SHARED_DIR/filename.txt')
    parser.add_argument('--numwords', type=int, required=True,
                        help='The number of words to print top frequency.')
    parser.add_argument('--storageaccount', required=True,
                        help='The name the Azure Storage account that owns the'
                             'blob storage container to which to upload'
                             'results.')
    parser.add_argument('--storagecontainer', required=True,
                        help='The Azure Blob storage container to which to'
                             'upload results.')
    parser.add_argument('--sastoken', required=True,
                        help='The SAS token providing write access to the'
                             'Storage container.')
    args = parser.parse_args()

    input_file = os.path.realpath(args.filepath)
    output_file = '{}_OUTPUT{}'.format(
        os.path.splitext(args.filepath)[0],
        os.path.splitext(args.filepath)[1])

    with open(input_file) as f:
        words = [word.strip(string.punctuation) for word in f.read().split()]

    word_counts = collections.Counter(words)
    with open(output_file, "w") as text_file:
        print('Word\tCount', file=text_file)
        print("------------------------------", file=text_file)
        for word, count in word_counts.most_common(args.numwords):
            print(word + ':\t' + str(count), file=text_file)
        print("------------------------------", file=text_file)
        print("Node: " + os.environ['AZ_BATCH_NODE_ID'], file=text_file)
        print("Task: " + os.environ['AZ_BATCH_TASK_ID'], file=text_file)
        print("Job:  " + os.environ['AZ_BATCH_JOB_ID'], file=text_file)
        print("Pool: " + os.environ['AZ_BATCH_POOL_ID'], file=text_file)

    # Create the blob client using the container's SAS token.
    # This allows us to create a client that provides write
    # access only to the container.

    # mooncake
    blob_client = azureblob.BlockBlobService(account_name=args.storageaccount,
                                             sas_token=args.sastoken,
                                             endpoint_suffix='core.chinacloudapi.cn')

    output_file_path = os.path.realpath(output_file)

    print('Uploading file {} to container [{}]...'.format(
        output_file_path,
        args.storagecontainer))

    blob_client.create_blob_from_path(args.storagecontainer,
                                      output_file,
                                      output_file_path)
