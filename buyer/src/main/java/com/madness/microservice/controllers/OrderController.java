package com.madness.microservice.controllers;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import javax.inject.Inject;
import javax.validation.Valid;
import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.POST;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import com.madness.microservice.models.Order;
import org.eclipse.microprofile.metrics.annotation.Timed;
import org.springframework.validation.annotation.Validated;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import com.google.gson.Gson;
import com.madness.microservice.infra.gson.GsonSerializer;
import com.madness.microservice.infra.mongo.MongoDbConnection;
import com.madness.microservice.infra.rabbitmq.RabbitMqConnection;

@Path("/buyer/orders")
@Produces(MediaType.APPLICATION_JSON)
@Consumes(MediaType.APPLICATION_JSON)
public class OrderController {

    private MongoDbConnection _conn;
    private Gson _gson;
    private RabbitMqConnection _rabbitMq;

    @Inject
    public OrderController(MongoDbConnection conn, GsonSerializer serializer, RabbitMqConnection rabbitMq) {
        this._conn = conn;
        this._gson = serializer.gson;
        this._rabbitMq = rabbitMq;
    }

    @POST
    public Response post(@Valid @RequestBody Order order) {
        var orders = _conn.collection("orders", Order.class);

        orders.insertOne(order);

        return Response.ok(_gson.toJson(order)).build();
    }

    @GET
    @Produces(MediaType.TEXT_PLAIN)
    @Timed
    public Response get() {
        var orders = _conn.collection("order", Order.class);

        var a = orders.find().cursor();

        List<Order> b = new ArrayList<Order>();

        while (a.hasNext()) {
            var or = a.next();
            b.add(or);
        }

        return Response.ok(_gson.toJson(b)).build();
    }
}