package service

import "sync"

import "log"

//Service interface used to represent a service
type Service interface {
	//Start the service
	Start()

	//Shutdown the service
	Shutdown()

	//Init anything that needs to be created before start is called
	Init()

	//Register all the broadcast this must be called before init
	Register()
}

var services []Service
var wg sync.WaitGroup

//Add a service to the pool of services
func Add(service Service) {
	service.Register()
	services = append(services, service)
}

//StartAll the services in the pool
func StartAll() {
	//Before starting all the services we need to init all one of them
	for _, service := range services {
		service.Init()
	}

	for _, service := range services {
		service.Start()
		wg.Add(1)
	}
}

//ShutdownAll the services in the pool
func ShutdownAll() {
	for _, service := range services {
		service.Shutdown()
	}
	log.Println("[Services] -> Waiting for all services to shutdown")
	wg.Wait()
	log.Println("[Services] -> All services are closed!")
}

// Closed called by a service once done
func Closed() {
	wg.Done()
}
