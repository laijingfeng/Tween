#!/usr/bin/python
# encoding=utf-8

import sys, shutil, os

def IsGoodBlock(block):
    for line in block:
        if line.find('method') != -1:
            return True
    return False

def GetTitle(block):
    if len(block) < 2:
        return ''
    ret = block[1].split(' ', 1)
    if len(ret) != 2:
        return ''
    return ret[1]

def GetParam(line):
    ret = line.split(' ', 4)
    if len(ret) != 5:
        return ['', '']
    if ret[3].find(':') == -1:
        return ['', '']
    return [ret[3].split(':',1)[0], ret[4]]

def GetReturn(line):
    ret = line.split(' ', 3)
    if len(ret) != 4:
        return ''
    return ret[3]

def HandleBlock(block):
    if IsGoodBlock(block) == False:
        return []
    ret = []
    ret.append('/// <summary>')
    ret.append('/// ' + GetTitle(block))
    ret.append('/// </summary>')
    for line in block:
        if line.find('* @param') != -1:
            pp = GetParam(line)
            if len(pp[0]) > 0:
                ret.append('/// <param name="' + pp[0] + '">' + pp[1] + '</param>')        
        elif line.find('* @return') != -1:
            ret.append('/// <returns>' + GetReturn(line) + '</returns>')
    return ret

if __name__ == '__main__':

    print 'start'

    filePath = ['LTDescr.cs', 'LeanTween.cs']

    for fPath in filePath:

        if os.path.exists(fPath) == False:
            continue
        
        ndata = []
        block = []
        inBlock = False
        
        with open(fPath, 'r') as f:
            read_data = f.readlines()
            for line in read_data:
                line = line.strip()
                if line.startswith('/*') == True and line.find('*/') == -1: # 同一行的不要
                    inBlock = True
                elif line.find('*/') != -1 and inBlock == True:
                    hblock = HandleBlock(block)
                    for hline in hblock:
                        ndata.append(hline)
                    block[:] = []
                    inBlock = False
                    continue
                
                if inBlock == False:
                    ndata.append(line)
                else:
                    block.append(line)

        shutil.copy(fPath, fPath + '.back')

        with open(fPath, 'w') as f:
            for line in ndata:
                f.write(line + '\n')
