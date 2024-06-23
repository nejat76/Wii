/*******************************************************************************
 * config.c
 *
 * Copyright (c) 2008 Magicus <magicus@gmail.com>
 * Copyright (c) 2009 Nicksasa <nicksasa@gmail.com>
 *
 * Distributed under the terms of the GNU General Public License (v2)
 * See http://www.gnu.org/licenses/gpl-2.0.txt for more info.
 *
 * Description:
 * -----------
 *
 ******************************************************************************/


#include <sys/stat.h>
#include <sys/types.h>
 
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <malloc.h>
#include <stdlib.h>
#include <gccore.h>
 
#include "tools.h"
#include "u8.h"

 
typedef struct _node
{
  u16 type;
  u16 name_offset;
  u32 data_offset; // == absolut offset från U.8- headerns början
  u32 size; // last included file num for directories
} U8_node;
 
typedef struct _header
{
  u32 tag; // 0x55AA382D "U.8-"
  u32 rootnode_offset; // offset to root_node, always 0x20.
  u32 header_size; // size of header from root_node to end of string table.
  u32 data_offset; // offset to data -- this is rootnode_offset + header_size, aligned to 0x40.
  u8 zeroes[16];
} U8_archive_header;
 
u16 be16(const u8 *p)
{
	return (p[0] << 8) | p[1];
}

u32 be32(const u8 *p)
{
	return (p[0] << 24) | (p[1] << 16) | (p[2] << 8) | p[3];
}

 
void write_file(void* data, size_t size, char* name)
{
	FILE *out;
 
	out = fopen(name, "wb");
	fwrite(data, 1, size, out);
	fclose(out);	
}
 
int read_sd(char *path, u8 **buffer)
{
	FILE *fp;
	fp = fopen(path, "rb");
	fseek(fp, 0, SEEK_END);
	u32 filesize = ftell(fp);
	fseek(fp, 0, SEEK_SET); 
	
	*buffer = allocate_memory(filesize);
	
	fread(*buffer, 1, filesize, fp);
	fclose(fp);
	return filesize;
}	
 
void do_U8_archive(u8 *buffer, char *path)
{
  U8_archive_header header;
  U8_node root_node;
	u32 tag;
	u32 num_nodes;
	U8_node* nodes;
	u8* string_table;
	size_t rest_size;
	unsigned int i;
	u32 data_offset;
	u16 dir_stack[16];
	int dir_index = 0;
	mkdir(path, 0777);
//	fread(&header, 1, sizeof header, fp);
	memcpy(&header, buffer, sizeof(header));
	tag = be32((u8*) &header.tag);
	if (tag != 0x55AA382D) {
	  printf("No U8 tag\n");
	}
    // printf("Header\n");
	 //sleep(2);
	//fread(&root_node, 1, sizeof(root_node), fp);
	memcpy(&root_node, buffer + sizeof(header), sizeof(root_node));
	num_nodes = be32((u8*) &root_node.size) - 1;
	//printf("Number of files: %d\n", num_nodes);
    //sleep(5);
	nodes = allocate_memory(sizeof(U8_node) * (num_nodes));
	//fread(nodes, 1, num_nodes * sizeof(U8_node), fp);
	memcpy(nodes, buffer+ sizeof(header)+ sizeof(root_node), num_nodes * sizeof(U8_node));
	//printf("Allocate mem & memcpy\n");
    //sleep(5);
	data_offset = be32((u8*) &header.data_offset);
	rest_size = data_offset - sizeof(header) - (num_nodes+1)*sizeof(U8_node);
 	//printf("REST SIZE\n");
	string_table = allocate_memory(rest_size);
	//fread(string_table, 1, rest_size, fp);
	memcpy(string_table, buffer+ sizeof(header)+ sizeof(root_node)+(num_nodes * sizeof(U8_node)), rest_size);
 	//printf("ENTERING LOOP\n");
	for (i = 0; i < num_nodes; i++) {
    U8_node* node = &nodes[i];   
    u16 type = be16((u8*)&node->type);
    u16 name_offset = be16((u8*)&node->name_offset);
    u32 my_data_offset = be32((u8*)&node->data_offset);
    u32 size = be32((u8*)&node->size);
    char* name = (char*) &string_table[name_offset];
    u8* file_data;
	char sd[1024];
	sprintf(sd, "%s/%s", path, name);
 
    if (type == 0x0100) {
      // Directory
	//printf("directory - making\n");
	//sleep(1);
      mkdir(sd, 0777);
	  printf("%s\n", sd);
      chdir(sd);
	  sprintf(path, "%s/%s", path, name);
      dir_stack[++dir_index] = size;
      //printf("%*s%s/\n", dir_index, "", sd);
	//sleep(1);
    } else {
      // Normal file
 
      if (type != 0x0000) {
         //printf("Unknown type\n");
      }
 	//printf("file - creating\n");
	//sleep(1);
     // fseek(fp, my_data_offset, SEEK_SET);
      file_data = allocate_memory(size);
      //fread(file_data, 1, size, fp);
	  memcpy(file_data, buffer + my_data_offset, size);
      write_file(file_data, size, sd);
      free(file_data);
      //printf("%*s %s (%d bytes)\n", dir_index, "", sd, size);
	//sleep(1);
    }
 
    while (dir_stack[dir_index] == i+2 && dir_index > 0) {
	//printf("chdir\n");
      chdir("..");
      dir_index--;
    }
	}
}

