sudo docker build -t webapp .
sudo docker run -d -p 8080:80 --name myapp webapp
