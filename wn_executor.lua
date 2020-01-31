if init then
    docker_arr("alpine", {"sh", "-c", "chmod +x "..task[1]})
end

local exitCode, stdout, stderr = docker_arr(global.image or "ubuntu:18.04", task)
return {
    stdout = stdout,
    stderr = stderr,
    exitCode = exitCode,
    task = task,
    global = global,
    out = folder(".")}